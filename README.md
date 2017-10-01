# SupercondActor Template Engine
#### Lightweight template engine for rendering elaborate emails using Json data####
> Use this engine to merge email template file with data expressed as Json object.
Usage example:
```
var templateContent = "Email from: {{firstName}} {{lastName}}";
var jsonData = "{'firstName': 'MyFirstName', 'lastName': 'MyLastName'}";
var result = _renderer.Render(templateContent, jsonData);
```
Resulting text:
```
Email from: MyFirstName MyLastName
```
## Template tags and features
Field referencing rules are based on the JSONPath notation. See examples in this [good JSONPath article](http://goessner.net/articles/JsonPath/) written by Stefan Goessner.

Feature | Template Tags | Description
------- | ------------- | -----------
Field value | {{Person.FirstName}} | Renders value of the field FirstName
Equivalent syntax | {{$.Person.FirstName}} | Renders value of the field FirstName
Item from array | [[$.orderLines]] Line {{LineNumber}} [[/$.orderLines]] | Renders content of the orderLines array
Reference to Parent | Line from order number {{$$$$.orderNumber}} | Renders field from parent object

