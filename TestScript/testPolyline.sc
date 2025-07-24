sqr[,] = { {0,0}, {10,0},{10,10},{0,10}};
cad.setLineType("solid");
cad.setLineThickness(1);
cad.setColor("Green");
cad.polygon(sqr[,]);
v[] = { 0,0,10};
cad.setColor("Yellow");
cad.polyline(v[],sqr[,]);
cad.disp();
