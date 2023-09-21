# vsEcommerce
.NET6 eCommerce Engine. vsEcommerce is free and open-source eCommerce engine with hyperlocal, marketplace solution and support for mobile, web or any platform. This code serve as a base for our SaaS product vsEcommerce

https://www.vsecommerce.com/ \
Documentation : https://help.vsecommerce.com/ \
API Documentation (Developer Doc) : https://help.vsecommerce.com/api/#introduction \
User Doc : https://help.vsecommerce.com/docs/overview/introduction \
Our Organization : https://www.vsonlineservices.com 

Our SaaS app Play Store : https://play.google.com/store/apps/details?id=com.vsonlineservices.vsecommerce \
Our SaaS application App Store : https://apps.apple.com/app/id1608689547 

# How to run vsEcom
Follow the steps below to run the vsEcommerce application,

### Run the Admin-Panel UI:

- Clone the "vsEcom-Admin-Panel" repository from [GitHub](https://github.com/vsonlineserv/vsEcom-Admin-Panel)
- Open the cloned project in visual studio code(vs Code)
- Then the run the following commands,
    - npm install
    - ng serve (to run the application)
- After running the application successfully

## Run the vsEcommerce backend:

- Clone the "vsEcommerce" repository from [GitHub]( https://github.com/vsonlineserv/vsEcommerce)
- Open VSOnline.VSECommerce.sln path to the file:vsEcommerce-main/src/ in visual studio 2022.
- Go to appsettings.json and replace the following parameters with your own credentials:
    - JwtKey
    - JwtIssuer
    - ConnectionStrings(Server, User Id, password)

- Configuring the Database in SQLServer (Microsoft Sql server management studio):

    - The above step can be done two ways, either  by,
    1. open sql server management studio and run the opensourceEcommerce.sql file(Located inside the script folder of the cloned repository)
    2. or by running the migration using the following commands after setting the connectionString correctly ,

        - add-migration migration-name
        - Update-Database

- Note: if you are going with running migration in the solution then you need to load the data in the tables(Country, Manufacturer, Seller, SellerBranch, User, Permissions) manually using the script file provided to you.

- After configuring the database, you can login with the below credentials for the vsEcom-Admin-Panel
    
    - Username: adminuser1@testvsecom.change
    - Password: testVSEcom@123

## Run the vsEcom-StoreFront-Angular:

- Clone the vsEcom-StoreFront-Angular repository from [GitHub](https://github.com/vsonlineserv/vsEcom-StoreFront-Angular)
- Open the cloned project in visual studio code(vs Code)
- Inside the repository extract the templates folder unzip them
- Then run "npm install", "ng serve" for one of the templates
- After successfully running the template you can find the products added in the admin panel displayed here.

- Note: If you need any parameter changes in API or any general API changes for that matter you can use the vsecom-storefront-services for that.

# License
Copyright (c) VS Online Services Pvt Ltd. All rights reserved.

Licensed under the VS Online Services Open Software License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://vsonlineservices.com/open-source-license

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
