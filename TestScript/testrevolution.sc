centerline[,] = { {0,0,0},{0,10,0}};
outline[,] = { {0,0,0}, {10,0,0},{15,10,0},{10,5,0}};
cad.setLineType("solid");
cad.setLineThickness(1);
cad.setColor("Yellow");
cad.revolution(centerline[,],outline[,]);
cad.disp();
