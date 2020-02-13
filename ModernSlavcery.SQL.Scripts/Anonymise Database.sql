DECLARE @DefaultPassword NVARCHAR(MAX) 
DECLARE @DefaultPIN NVARCHAR(MAX) 
DECLARE @DefaultSecurityCode NVARCHAR(MAX) 
DECLARE @DefaultPhoneNumber NVARCHAR(MAX) 
SET @DefaultPassword='Cadence2007'
SET @DefaultPIN='ABCDEF'
SET @DefaultSecurityCode='ABCD1234'
SET @DefaultPhoneNumber='01234567890'

--Set a global password without using a hashing algorithm
UPDATE Users SET PasswordHash=@DefaultPassword, HashingAlgorithm=-1

--Anonymise the Name and email address
UPDATE U 
SET Firstname='Firstname' + CAST(U.UserId AS NVARCHAR), Lastname='Lastname' + CAST(U.UserId AS NVARCHAR), EmailAddress='email' + CAST(U.UserId AS NVARCHAR) + '@testdomain.com' 
FROM Users U 

--Anonymise the contact email address
UPDATE U 
SET ContactEmailAddress=U.EmailAddress
FROM Users U 
WHERE ContactEmailAddress IS NOT NULL AND RTRIM(LTRIM(ContactEmailAddress))<>''

--Anonymise the contact phone number
UPDATE U 
SET ContactPhoneNumber=@DefaultPhoneNumber
FROM Users U 
WHERE ContactPhoneNumber IS NOT NULL AND RTRIM(LTRIM(ContactPhoneNumber))<>'';

--Anonymise the contact first name
UPDATE U 
SET ContactFirstName=U.Firstname
FROM Users U 
WHERE ContactFirstName IS NOT NULL AND RTRIM(LTRIM(ContactFirstName))<>''

--Anonymise the contact last name
UPDATE U 
SET ContactLastName=U.Lastname
FROM Users U 
WHERE ContactLastName IS NOT NULL AND RTRIM(LTRIM(ContactLastName))<>'';

--Anonymise the organisation security codes
UPDATE Organisations SET SecurityCode=@DefaultSecurityCode
WHERE SecurityCode IS NOT NULL 

--Anonymise the PIN codes
UPDATE UserOrganisations SET PINHash=NULL, PIN=@DefaultPIN
WHERE PINHash IS NOT NULL OR PIN IS NOT NULL

--Anonymise the feedback email address
UPDATE F 
SET EmailAddress='email' + CAST(F.FeedbackId AS NVARCHAR) + '@testdomain.com' 
FROM Feedback F

--Anonymise the feedback phone number
UPDATE F 
SET PhoneNumber=@DefaultPhoneNumber
FROM Feedback F 
WHERE PhoneNumber IS NOT NULL AND RTRIM(LTRIM(PhoneNumber))<>''




