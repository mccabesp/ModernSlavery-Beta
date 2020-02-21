# Modern Slavery Reporting service

This is the code for the [Modern Slavery Reporting service](https://modern-slavery.service.gov.uk).

From 2017, any organisation that has 250 or more employees must publish and report specific figures about their Modern Slavery. 
Modern Slavery is the difference between the average earnings of men and women, expressed relative to men’s earnings. 
For example, ‘women earn 15% less than men per hour’.

This service allows organisations to report their Modern Slavery statement. It then makes this data available to the general public. 
It is managed by the Government Equalities Office (GEO) which forms part of the Cabinet Office.


## Technical documentation

This is a C# solution that has a user interface for public viewing pages, organisation reporting pages and an admin portal. There are also
regular jobs for pulling information from Companies House and sending reminder emails. We use Identity Server for authentication 
and SQL Server for the database.

### Dependencies

- [GOV.UK Notify](https://www.notifications.service.gov.uk/) - to send emails and letters
- [Companies House API](https://developer.companieshouse.gov.uk/api/docs/index.html) - to look up information for organisations

## Licence

[MIT License](LICENCE)
