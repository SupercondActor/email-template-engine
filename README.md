# SupercondActor Template Engine
#### Lightweight template engine for rendering elaborate emails using Json data####
> Use this engine to merge email template file with data expressed as Json object.

##Usage example:
```
var templateContent = "Email from: {{firstName}} {{lastName}}";
var jsonData = "{'firstName': 'MyFirstName', 'lastName': 'MyLastName'}";
renderer = new TemplateRenderer();
var result = renderer.Render(templateContent, jsonData);
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

## Another example
Template text:
```
<h1>Dear {{$.fname}} {{$.lname}},</h1>
Thank you for your order.<br/>
Your total amount is ${{$.Amount ~ 0000.0}}, Date: {{$.Date~d}}

<p>Ordered items</p>
[[$.OrderLines  ]]
Item {{$.LineNumber}}: {{$.Item}}, your price: {{$.Price~C}} <br/>
 Accessories:<br/>
[[$.Accessories]] {{$.Number}}: {{$.Name}} for {{$$$$.Item}}
[[/$.Accessories ~ No accessories found for {{$.Item}}]][[/$.OrderLines]]

Note the iPad price: {{$.OrderLines[?(@.Item == 'iPad')].Price~c}}

<p>Thank you.</p>
```
Data Json:
```
{'fname':'John', 'lname':'Doe', 'Amount': 13.25, 'Date': '2013-10-21T13:28:06.419Z',
'OrderLines': [
	{ 'LineNumber': 1, 'Item': 'iPhone', 'Price': 600.01,
		'Accessories': [
			{ 'Number': 1, 'Name': 'Cord' },
			{ 'Number': 2, 'Name': 'Cover' }
		]},
	{ 'LineNumber': 2, 'Item': 'iPad', 'Price': 900.99 }
]
}
```
Result:
```
<h1>Dear John Doe,</h1>
Thank you for your order.<br/>
Your total amount is $0013.3, Date: 10/21/2013

<p>Ordered items</p>

Item 1: iPhone, your price: $600.01 <br/>
 Accessories:<br/>
 1: Cord for iPhone
 2: Cover for iPhone

Item 2: iPad, your price: $900.99 <br/>
 Accessories:<br/>
 No accessories found for iPad

Note the iPad price: $900.99

<p>Thank you.</p>
```
For more examples see UnitTests project in this repository.