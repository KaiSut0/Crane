Crane is a Grasshopper plugin to design origami products.

Designing foldable products with rigid folding motion is difficult for designers, architects, and researchers who are not origami experts. Crane is a powerful tool for not only such people but also origami experts [1]. This software focuses on design, rigid folding simulation, form-finding, and fabrication for origami. Because Crane Solver is a hard constraint solver, it can precisely simulate a rigid folding motion and form-finding with geometrical constraints like Freeform Origami [2][3].

Developer:
Kai Suto (Origami Lab/Tachi Lab, Nature Architects Inc.)
Kotaro Tanimichi (Nature Architects Inc.)
Kanata Warisaya (Origami Lab/Tachi Lab)

How to Install Crane:
- Using PackageManager (Rhino7 only)
1. Open Rhino7
2. Type "PackageManager" as RhinoCommand
3. Search Crane and install it.
4. Then play some example files that can be downloaded from this page.
- Manual install (Rhino6 and Rhino7)
1. Download crane_xxx.zip from Crane_ver_xxx
2. "Unblock" crane_xxx.zip (for more detail, please check "Troubleshooting".) and unzip it.
3. Open the Library folder and paste all files in "Crane_xxx/Libraries/Crane_xxx" into it.
4. Open the UserObjects folder and paste all files in "Crane_xxx/UserObjects/" into it.
5. Then play some example file at "Crane_xxx/Examples".

How to use Crane:
1. Create an initial origami crease pattern using tessellation components or manual drawing.
2. Simulate the rigid folding motion using the crane solver.
3. Find the form that satisfies given geometrical constraints using the crane solver and constraint components.
4. Generate cutting lines or solids for a CNC, a laser cutter, or a 3d printer using fabrication components.

External Libraries:
1. MathNet.Numerics for linear algebra operations
2. OpenCvSharp for treating row image of crease patterns

Troubleshooting:
If you have a missing components problem, please check the below.
1. Check whether you unblocked the "crane10.zip" before unzipping it.
2. Right-click crane10.zip and select "properties", then uncheck the "Unblock" check box.
3. Then you can reinstall Crane and it will work well.

License:
Crane is a proprietary software provided under the following license.
1. The user allowed to use the software if and only if both of the following conditions are satisfied.
    * (Non-Commercial Use): The use of the software is non-commercial.
    * (Attribution): The resulting works achieved using the software, e.g. research publications, exhibitions, education workshops, etc. acknowledge the usage of the software and our names(Kai Suto, Kotaro Tanimichi)
2. Any other type of usage, e.g., commercial use, is by default prohibited. For this type of usage, please contact: dqm2kai@gmail.com

Acknowledgments:
The developers were supported by "the Mitou IT talent scout and development project" for the development of this software.
We would like to express our very great appreciation to Tomohiro Tachi for teaching the principle of rigid-folding/form-finding simulations and providing machine tools.
We would like to thank Yuta Shimoda for providing the design of the foldable chair.
We would like to thank A-POC ABLE ISSEY MIYAKE for the great collaboration in applying a computational design to "Steam Stretch" to make almost any 3d shape from a piece of cloth [4]. These results were exhibited at the 2023 Milan Design Week.

Reference:

[1] Kai Suto, Yuta Noma, Kotaro Tanimichi, Koya Narumi, Tomohiro Tachi, "Crane: An Integrated Computational Design Platform for Functional, Foldable, and Fabricable Origami Products", https://dl.acm.org/doi/10.1145/3576856
[2] Tomohiro Tachi, "Freeform Origami", www.tsg.ne.jp/TT/software/
[3] Tomohiro Tachi, "Freeform Variations of Origami", in Proceedings of The 14th International Conference on Geometry and Graphics (ICGG 2010), Kyoto, Japan, pp. 273--274, August 5-9, 2010.
[4] A-POC ABLE ISSEY MIYAKE, Nature Architects, THINKING DESIGN, MAKING DESIGN: TYPE-V Nature Architects project by A-POC ABLE ISSEY MIYAKE, 2023, Milan Design Week, Milan, Italy
