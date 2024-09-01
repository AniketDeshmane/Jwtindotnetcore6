
# JWT Authentication in ASP.NET API

For using Sqlite with Entity core, refer this project 
```
https://github.com/AniketDeshmane/APIwithSqliteDb.git
```

Step 0: Add the dependancy reference

```
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="6.0.33" />
```

Step 1: In the [appsetting.json] file add the Issuer,Audience,Subject and Key
```
  "JWT": {
    "Issuer": "amd401",
    "Audience": "amd401",
    "Subject": "amd401",
    "SigningKey": "fHHIeNZDGp17aakM5I4veflIu4E8wCHt9tcvlVHrdGwRqxEY0SLbFYll9b1WlBqz"
  }
```
Step 2: Create the model class [UserDetails.cs]
```
   public class UserDetails
   {
       [Key]
       public int UserID { get; set; }

       [Required]
       public string UserName { get; set; } = string.Empty;

       [Required]
       public string Password { get; set; }
   }
```

Step 3: Add/Create the DataContext class and DbSet [DataContext.cs]
```
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<UserDetails> Users => Set<UserDetails>(); //add this Dbset
    }
```

Step 4: Add the Services and configure them in [Program.cs]
```
//Add the service for DataContext
builder.Services.AddDbContext<DataContext>
                                        (options => 
                                        options.UseSqlite(
                                            builder.Configuration.GetConnectionString("DefaultConnectionString")
                                            )
                                        );

//Add the serivces for JWT Token Checking
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                              options.TokenValidationParameters = new TokenValidationParameters
                              {
                                  ValidateIssuer = true, // Ensures the token's issuer is validated.
                                  ValidIssuer = builder.Configuration["JWT:Issuer"], // Sets the expected issuer of the token.
                                  ValidateAudience = true, // Ensures the token's audience is validated.
                                  ValidAudience = builder.Configuration["JWT:Audience"], // Sets the expected audience of the token.
                                  ValidateLifetime = true, // Ensures the token hasn't expired.
                                  ValidateIssuerSigningKey = true,
                                  IssuerSigningKey = new SymmetricSecurityKey
                                                        (Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])
                                                        ) // Sets the SymmetricSecurityKey to validate the token's signature.
                              }); 
```

Step 5: Add the controller and the action methods

PART A) Create a controller named [UserController.cs] and implement the [Signup] route to register the user in the system
```
        [HttpPost]
        [Route("Signup")]
        public async Task<IActionResult> Signup([FromBody] UserDetails user)
        {
            _dataContext.Users.Add(user);
            await _dataContext.SaveChangesAsync();
            return Ok("User created successfully");

        }

```
PART B) Implement the [Signin] route to validate the user by the database and generate the JWT Token if the user is valid 
```
[HttpGet("Signin")]
public async Task<IActionResult> Signin([FromBody] UserDetails user)
{
    var userinfo = await _dataContext.Users.FirstOrDefaultAsync(x => x.UserName == user.UserName && x.Password == user.Password);
    if (userinfo == null)
    {
        return NotFound("Incorrect Username or Password"); // Return 404 if the user is not found
    }

    //Add logic to generate the jwt token

    //claims are the payload in the JWT Token  
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub,_configuration["JWT:Subject"]),
        new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),

        new Claim("UserID", userinfo.UserID.ToString()),
        new Claim("UserName", userinfo.UserName.ToString()),

    };

    var SigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"]));

    var signInAlgoAndKey = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        _configuration["JWT:Issuer"],
        _configuration["JWT:Audience"],
        claims,
        signingCredentials: signInAlgoAndKey,
        expires: DateTime.UtcNow.AddMinutes(10)
    );

    var JwtTokenValue = new JwtSecurityTokenHandler().WriteToken(token);

    return Ok(new { JwtTokenValue, userinfo }); // Return 200 OK status
}
```
PART C) To access the other api's decorate the action method with [Authorize] filter.
We should pass the valid token generated from the [Signin] endpoint
Once the valid JWT is passed then its will return the requested resource from the database
```
        [HttpGet("GetUserByUsername")]
        [Authorize]
        public async Task<IActionResult> GetUserByUsername([FromQuery] string username)
        {
            var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            return Ok(user);
        }
```

# JWT Token

```
[eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9].
[eyJzdWIiOiJhbWQ0MDEiLCJqdGkiOiJkZTAwMWY0Ny1hMTJkLTRlOTItOWE0Yy0zMTVkZmU1MjFmOWEiLCJVc2VySUQiOiIzIiwiVXNlck5hbWUiOiJBYmhpbmF2IiwiZXhwIjoxNzI1MTc4MzgzLCJpc3MiOiJhbWQ0MDEiLCJhdWQiOiJhbWQ0MDEifQ].
[hJAgW8gRjaDogbWDMpMAg-JqTWkquv6Dj2911FcHllw]
```
1. We can decode the JWT token using this website
```
https://jwt.io/
```
2. JWT Token Consist of 3 parts
Part 1: HEADER:ALGORITHM & TOKEN TYPE
```
{
  "alg": "HS256",
  "typ": "JWT"
}
```
Part 2: PAYLOAD:DATA (Claims,Issuer,Audience,Expiry)
```
{
  "sub": "amd401",
  "jti": "de001f47-a12d-4e92-9a4c-315dfe521f9a",
  "UserID": "3",
  "UserName": "Abhinav",
  "exp": 1725178383,
  "iss": "amd401",
  "aud": "amd401"
}
```
Part 3: VERIFY SIGNATURE (Signing Key)
```
HMACSHA256(
  base64UrlEncode(header) + "." +
  base64UrlEncode(payload),
  
your-256-bit-secret

)
```

