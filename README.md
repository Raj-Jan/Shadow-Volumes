# Shadow-Volumes

Project uses sharpDX 11 to implement depth-stencil shadow volume technique to create accurate shadows in real time. 
This technique produces shadows with very sharp edges without any jagged edges, but is slower than regular shadow mapping.
Efficient implementation requaires meshes with adjectancy data to detect shilluette edges. I used custom mesh file extension
to store meshes with precalculated adjectany data (converting 16k vertex mesh took about 10s), which made it possible to load file nearly instantly.

![Alt Text](result.gif)
