= ITEM CLASSES = 

- The BaseItem class will be the underlying class all items derive from in the inheritance heirarchy.
- Each BaseItem class contains data all items will need (3d model, 2d icon, etc)
- Each BaseItem can also have a list of crafting recipies

- NOTE: Crafting system for the BaseItem is tentative, will change depending on what we discuss and finalise on later down the line

- Each item's 3D model should also have the ItemModelScript Component. 
- This component will allow us to drop/pickup our Scriptable-Object BaseItems into a MonoBehaviour GameObject with ease.
- Each ItemModelScript contains a reference to it's respective BaseItem, allowing us to drop it from and add it back to lists easily.




- Aaron