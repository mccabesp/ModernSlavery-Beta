# Modern slavery statement registry

This is the code for the [Modern slavery statement registry](https://modern-slavery-statement-registry.service.gov.uk/).

Since 2015, the UK Modern Slavery Act has required large businesses to publish an annual statement setting out the steps they are taking to prevent modern slavery in their operations and supply chains. 

This service allows any organisation to submit a summary of their annual Modern Slavery statement to GOV.UK. It then makes this summary available to the general public for recognition and scrutiny. 
The service is managed by the Modern Slavery Unit which forms part of the Home Office.


## Technical documentation

This is a C# cloud solution that includes user account set-up, registration of organisations, annual submitting of statements, a public interface for viewing pages as well as an administrator portal. There are also
regular jobs for updating organisation information from Companies House and sending emails. The solution uses Identity Server for authentication and SQL Server for the database.

### Dependencies

- [GOV.UK Notify](https://www.notifications.service.gov.uk/) - to send emails and letters
- [Companies House API](https://developer.companieshouse.gov.uk/api/docs/index.html) - to look up information for organisations

## Licence

[MIT License](LICENCE)
