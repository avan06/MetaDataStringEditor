# global-metadata.dat Partial string modification tool
&emsp;&emsp;For Android games exported by the Unity-il2cpp script backend, the strings appearing in the code will be compiled into the assets\bin\Data\Managed\Metadata\global-metadata.dat file. As a part of the localization work, I simply took a tool Modify the strings in it.

## References
- [il2cppdumper](https://github.com/Perfare/Il2CppDumper)<br>
The understanding of the content of this file is learned from the source code of this tool. The tool itself is used to export the class definition from the compiled libil2cpp.so file and global-metadata.dat file. Renaming scripts, UABE, and DLLs available for AssetStudio is a great tool.

## Modify content
&emsp;&emsp;In global-metadata.dat, the way to save the strings in the code is that there is a list in the header to put the offset, length and other information of each string, and then there is an area in the data area to compactly put all the A string with a list of heads, so no need to end with file.<br>
&emsp;&emsp;Because the number of strings does not change before and after the modification, the modification of the list is directly overwritten on the original area. The length of the data area may change. If the length of the data area is less than or equal to the original length after the modification, the data is directly overwritten. If it is too long, it is written to the end of the file.

## New Feature
- Added a feature to display the index numbers in the ListView.  
This provides users with a fast and clear way to know the index number of their text entries.

- Added support for drag and drop file feature.  
Allowing users to quickly drag and drop the "global-metadata.dat" file into this program and open it.

- Added support for convenient export and import of text feature.  
After exporting the text, the content can be modified using a text editor, and the modified phrases will be immediately displayed in the ListView after importing.

### Export Text Format
```
global-metadata: <index 1>
<text 1>
global-metadata: <index 2>
<text 2>
global-metadata: <index 3>
<text 3>
...
```

### Import Text Format
- Unused content can be removed.  
- Modified text can be placed on the second line, or  
- directly modified in the first line.

```
global-metadata: <index 2>
<text 2>
<modified text 2>
global-metadata: <index 3>
<modified text 3>
...
```

![main](main.png)