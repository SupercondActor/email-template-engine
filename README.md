# SupercondActor Template Engine
#### Lightweight template engine for rendering elaborate emails using Json data
> Use this engine to merge email template file with data expressed as Json object.

## Usage example:
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
Items from array | [[$.orderLines]] Line {{LineNumber}} [[/$.orderLines]] | Renders content of the orderLines array
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

## Templates and data
To create a beautiful email, you need two things:
1.	A template describing email layout
2.	A data object holding values for the placeholders in the template
You call Renderer method to merge template with the data:
var result = renderer.Render(templateContent, jsonData);
As the result, you'll get the content of the email body. Actually, if you want to place some data in the email's subject, you can also create a simple template to render the subject string.
## Data object
You can provide the data for your template as pretty much any .NET object. Internally it will be serialized as Json anyway. You can also supply your data as a Json string - this option is very useful when you store the data in a database or a file.
There are practically no restrictions on the shape of your data object. It can be plain object with a number of fields, or an array of objects, or an object holding nested objects or nested arrays. You just need to know how to write a reference to a field in the template.
## Referencing data fields in the template text.
Field referencing rules are based on the standard JSONPath notation. See examples in this [good JSONPath article](http://goessner.net/articles/JsonPath/) written by Stefan Goessner.
In the SupercondActor Template Engine we extended JSONPath with the ability to reference parent fields from a child's context (see examples below).
## Template notation
To make a simple reference in the template you use double curly braces notation: {{myFieldName}}.
To render an array of items use double square brackets: [[$.arrayName]] {{item}} [[/$.arrayName]]
## Simple reference to a plain object field
Let's say you have a simple data object serialized as Json:
```
{"firstName": "MyFirstName", "lastName": "MyLastName"}
```
You can reference that data in a template like this:
```
Email from: {{firstName}} {{lastName}}
```
Alternatively, you could use full JSONPath notation, where dollar sign refers to the root of current context:
```
Email from: {{$.firstName}} {{$.lastName}}
```
The full JSONPath notation is useful, for example, when your data object is an array, so you need a way to reference the root array which does not have a field name assigned
You handle nested data objects using the usual JavaScript notation. Here is the example template:
```
From deep: {{Address.Person.firstName}} {{Address.Person.lastName}}
```
## Rendering an array of objects
If your data contains an array of items, you can iterate through the array using double square brackets to specify the field holding the array. For example, in this data object the array's field name is 'orderLines':
Data object:
```
{'name': 'John',
'orderLines': [
{ 'LineNumber': 1, 'Item': 'iPhone', 'Price': 600.0199 },
{ 'LineNumber': 2, 'Item': 'iPad', 'Price': 900.99 },
{ 'LineNumber': 3, 'Item': 'iTV', 'Price': 1200 }
]
}
```
The template:
```
<h1>Dear {{name}},</h1>
<p>Here is your order:</p>
[[$.orderLines]]
Item {{$.LineNumber}}: {{$.Item}}, your price: {{$.Price~C}} <br/>[[/$.orderLines]]
```
Note, that here the dollar sign specifies current context: in the [[$.orderLines]] the $ refers to the root data object, in the line description the $ refers to the child context - the current line. To refer to a field in the parent context from a child one you need to add more dollar signs to the notation, see the details in the sections below.
## Field formatting
You can specify a format for numeric and date/time fields, for example {{$.Price~C}}. You need to separate the field name from the format string using the '~' sign. In this example we format number as currency. The format definition string can be anything you would use in the Decimal.ToString("#.00") C# method call.
Referencing parent fields from a child context
Sometimes when rendering an array, you need to refer to a field form a parent object, like referencing Order Number inside an order Line data. To move up the context chain just add more dollar signs in front of the field name.
Data:
```
{'OrderLines': [
{ 'LineNumber': 1, 'Item': 'iPhone', 'Price': 600.01,
'Accessories': [
{ 'Number': 1, 'Name': 'Cord' },
{ 'Number': 2, 'Name': 'Cover' }
]},
{ 'LineNumber': 2, 'Item': 'iPad', 'Price': 900.99 }
]
}
```
Template:
```
<p>Ordered items</p>
[[$.OrderLines  ]]
Item {{$.LineNumber}}: {{$.Item}}, your price: {{$.Price~C}} <br/>
 Accessories:<br/>
[[$.Accessories]] {{$.Number}}: {{$.Name}} for {{$$$$.Item}}
[[/$.Accessories ~ No accessories found for {{$.Item}}]][[/$.OrderLines]]
```
Note the '{{$$$$.Item}}' placeholder here. It refers to a parent Item from a member of the child 'Accessories' array.
Yes, the four $ signs look confusing, but this is how Json.NET library works. Every $ moves you up one step through the Json parent/child chain. As a rule of thumb, put twice as many $-s as you would consider logical.
## Processing empty arrays
In the example above you may notice the [[/$.Accessories ~ No accessories found for {{$.Item}}]] notation. Double square brackets with slash specify the closing tag, the end of array rendering. Here you can provide a template fragment that should be displayed when the array does not have any items. Separate the template fragment from the array name using the '~' sign.
 