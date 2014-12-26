﻿# Localised Resource Extraction Benchmark

Here's a quick link to the [results](#results).

One of the systems I worked on was a statistical datawarehouse application. Each dataset has several dimensions, each dimension has several dimension members, and each dimension member can have labels in multiple languages.

I wanted to investigate the fastest way of extracting the labels. In particular, the case where labels in all languages are required. The system is years old, and schema changes are very difficult to do, so I only investigated extracting from our current schema. I did not investigate if there are schema changes that could make it more faster.

I made all of the extraction methods need to return a list of immutable, strongly typed objects, to ensure that quicker SQL queries aren't being traded off for longer processing in .NET.

## Schema

You can check the DbInitialise class in the Setup folder for the create table definitions. A quick description is that there's a single dictionary table that contains all labels for everything with translations. Tables that contain objects with labels have a dictionary key, to look up in the dictionary table. In this benchmark application, there is only a single source table, so the dictionary table and source table IDs match. However, this is not the case in our production system, hence why I don't have a foreign key relationship between the tables.

## Extraction methods

### Single Language
This is added as a reference to compare how much faster it is to extract the source data in only a single language.

### Basic Join
This performs a single join on the dictionary table, so the result set contains multiple lines per source row. The extractor needs to find matches, to add to the object's label dictionary, before returning the result.

### Basic Join As XML
The exact same query as the Basic Join, just with SQL Server's "FOR XML AUTO" added to the end. This means the .NET code didn't need to find duplicate rows, but instead does XML processing.

### Labels As Columns
This generates a dynamic SQL query, based on the numer, and names, of the languages the application is configured for. This means there's just one row in the result set for each row in the source table, and the SQL database needs to transfer less data to the application, since there's significantly less duplicated data.

### Labels As XML
This query also returns one row in the result set for each row in the source table. It also has a static result set structure. It does this by doing a sub query (could call a scalar value function) that retuns all the labels as XML in the label column of the result set. The application then needs to parse the XML and transform it into the application's model data structure.

## <a id="results">Results</a>

|Extraction Method|Time to first result|Time to complete|
|-----------------|--------------------|----------------|
|SingleLanguage|0.05s (288%)|0.07s (72%)|
|LabelsAsColumns|0.02s (100%)|0.1s (100%)|
|BasicJoin|0.02s (103%)|0.15s (154%)|
|BasicJoinAsXml|0.02s (117%)|0.21s (220%)|
|LabelsAsXml|0.01s (43%)|0.32s (327%)|