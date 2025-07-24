poly0[,] = { {0,0,0},{10,0,0},{10,10,0},{0,10,0},{0,0,0} };
poly1[,] = { {0,0,5},{12,0,5},{10,10,5},{-2,12,5},{0,0,5} };
cad.setLineType("solid");
cad.setLineThickness(1);
cad.setColor("Green");
cad.blend(poly0[,],poly1[,]);
cad.disp();
