# Skater XL Map Tools
Unity template project and a collection of tools for Skater XL custom map creation.

## Requirements
* Unity 2018.4.12f1
* Skater XL

## Features
### Export Tool
Builds your open scene to an AssetBundle and copies the file to the Skater XL custom maps directory in My Documents, ready to play! 

![ExportTool](https://i.imgur.com/afqJXVK.jpg)

### GrindSurface
This component aims to streamline the creation of grindable objects, including generation of primitive colliders. 

* Add a GrindSurface component to any object, then use the buttons in the inspector to draw or manually add GrindSplines as children. When drawing splines, colliders are automatically generated on confirmation.

###  GrindSpline
This component makes setting up grindable objects a little easier, with visual gizmos and some extra steps during the export process, allowing for more flexible scene setups. 

* Add a GridSpline component to a GameObject, use the "Add Point" button to create points for the grind spline
* The GrindSpline component will automatically update the name of the object so that it gets picked up correctly by the importer
* When Exporting, all objects with a GrindSpline component are moved into a "Grinds" object in the root of the scene, in order for the importer to detect and process them correctly.

![GrindSpline](https://i.imgur.com/XuoMo8H.jpg)
