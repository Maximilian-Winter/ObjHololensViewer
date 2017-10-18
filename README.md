# ObjHololensViewer
# An unity based application to load, view, scale and move around .obj files with the hololens.
Show the user a list of all avaible .obj files in ApplicationData.Current.RoamingFolder.Path, to select a file to load.
After loading is finished the user can move and scale the model with the help of a ui.

#Limitations:
-Only support OBJ files with vertex normals.
-Only assign "kd" color attribute from MTL files to the models.
