# Solar Watch 
Solar Watch is a lightweight sunset/sunrise checking web app. It was the advanced ASP.NET module solo project where we added to it week-by-week through 5 weeks. The project was designed to teach us ASP.NET web API with database connection and React frontend with dockerisation and deployment.

# Table Of Contents
- [Used technologies](#used-technologies)  
- [Features](#features)  
- [Installation](#installation)   
- [Usage](#usage)

# Used technologies  
![Static Badge](https://img.shields.io/badge/ASP.NET-red?logo=.net) ![Static Badge](https://img.shields.io/badge/C%23-red?logo=c%23) ![Static Badge](https://img.shields.io/badge/Entity%20Framework-red?logo=dotnet%20entity) ![Static Badge](https://img.shields.io/badge/Identity-red?logo=identity)



![Static Badge](https://img.shields.io/badge/React-blue?logo=react) ![Static Badge](https://img.shields.io/badge/Javascript-blue?logo=javascript)
 ![Static Badge](https://img.shields.io/badge/Vite-blue?logo=vite) ![Static Badge](https://img.shields.io/badge/NPM-blue?logo=npm) ![Static Badge](https://img.shields.io/badge/StyledComponents-blue?logo=styledcomponents)


 ![Static Badge](https://img.shields.io/badge/Database-ADS%20MSSQL-black) ![Static Badge](https://img.shields.io/badge/Docker-black?logo=docker)

 


# Main features  
  
- registration/login/logout
- jwt token authentication and authorization
- logged in users can start a query with city data and a date - first the city data is checked in the database and if it's not there yet, the data is accessed through OpenWeatherMap GeocodingApi to access coordinates - both country and state names are converted into their ISO abbreviations for this step. With the city data converted into coordinates the sunset/sunrise data can be accessed.
- with each user request the time zone of the location is sent together with the solar date in a combined DTO on the frontend this data is used to convert the time, that is first shown in the user's system timezone, to local time

# Installation   
If you'd like to see the deployed version you can visit [here](https://solarwatchfrontend-fzg9gec4dqd4gegx.polandcentral-01.azurewebsites.net/) hosted on Microsoft Azure. (disabled until further notice)

If you'd like to install and run the development version, you're going to need to take the following steps:
1. Prerequisites:
   - Backend software and package versions:
      - .NET 8.0 SDK
      - DotNetEnv	^3.1.0	
      - Microsoft.AspNetCore.Authentication.JwtBearer	^8.0.7	
      - Microsoft.AspNetCore.Identity.EntityFrameworkCore	^8.0.7	
      - Microsoft.AspNetCore.OpenApi ^8.0.7	
      - Microsoft.EntityFrameworkCore	^8.0.7
      - Microsoft.EntityFrameworkCore.Design	^8.0.7 
      - Microsoft.EntityFrameworkCore.SqlServer	^8.0.7	
      - Swashbuckle.AspNetCore	^6.4.0	
   - Frontend software and package versions:
      - Node.js ^22.2.0
      - Npm ^10.7.0
      - React	^18.3.1
      - React-dom	^18.3.1
      - React-router-dom	^6.25.1
      - Styled-components	^6.1.12
      - tsparticles ^3.5.0
      - @tsparticles/react ^3.0.0

Clone the repository and download the above packages. 

You're gonna need to setup an appsettings.Development.json on the backend to set your variables for the db connection and the jwt token settings. It should have the following structure, so the code can interpret it the right way:
```json
  "ConnectionStrings": {
    "Default": "Server=localhost,1433;Database=SolarWatch;User Id=sa;Password=[YOUR_SA_PASSWORD];Encrypt=False;"
  },
  "Jwt": {
    "ValidIssuer": "here comes the valid issuer you'd like to set for the jwt token generation",
    "ValidAudience": "here comes the valid audience you'd like to set for the jwt token generation",
    "IssuerSigningKey": "here comes the issuer signing key you'd like to set for the jwt token generation"
  },
  "Roles": {
    "Admin": "here comes the name you'd like to use for admin roles",
    "User": "here comes the name you'd like to use for user roles"
  },
  "Secret": {
    "admin_pw": "here comes the password you'd like to set for the deafult admin",
    "OPEN_WEATHER_APIKEY": "here comes your api key**"
  }
```
**claim your free key at [here](https://home.openweathermap.org) you need to create an account and after that you'll receive your key in an email, you'll have to wait a bit for it to activate.
After that, you can start the application two different ways:
1. With docker-compose:
   - Fill in your username and password in the provided docker-compose-pattern.yml both backend/environment/CONNECTIONSTRINGS__DEFAULT and db/environment/MSSQL_SA_PASSWORD
   - From the root folder: ```cd SolarWatch```
   - In the terminal: ```docker compose up```
3. Running separately the frontend and the backend on your local machine:
   - Backend:
       - From the root folder: ```cd SolarWatch/SolarWatch```
       - In the terminal: ```dotnet run```
    - Frontend:
       - From the root folder: ```cd SolarWatch/Frontend```
       - In the terminal: ```npm run dev```
    - Make sure your referenced database server is also running
     

# Usage 

1. Register an account
 
 ![image](https://github.com/user-attachments/assets/326a3cad-4ea8-403e-8f51-cd160ab16ace)

 ![image](https://github.com/user-attachments/assets/ecafe645-3eb4-45c4-aa0d-1f4426856eeb)
  

2. Log in
  
![image](https://github.com/user-attachments/assets/f89fdac2-c686-4313-b747-840cba225974)

  
3. Query a city and a date
  
![image](https://github.com/user-attachments/assets/7b431d50-4b30-4937-a3b4-82b8a4555954)

![image](https://github.com/user-attachments/assets/3c3e9189-3333-40ae-b0f5-e1af3b7cc328)

![image](https://github.com/user-attachments/assets/edbcc976-8266-4899-aaa8-431aeeaaeb92)







