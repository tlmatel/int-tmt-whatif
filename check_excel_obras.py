import xlrd

wb = xlrd.open_workbook('2605_potencial_core.xls')
sh = wb.sheet_by_name('Obras')

print(f"Columnas: {[sh.cell_value(0, c) for c in range(sh.ncols)]}")
print(f"\nTodas las filas de Obras:")
print(f"{'Codigo':<8} {'Nombre':<30} {'Provincia':<10} {'Producto':<10} {'Bloq':<5} {'Partner':<15} {'Usuarios':<8}")
print("-" * 100)

for r in range(1, sh.nrows):
    code = int(sh.cell_value(r, 0)) if sh.cell_value(r, 0) else ''
    nombre = sh.cell_value(r, 1)
    prov = sh.cell_value(r, 2)
    producto = sh.cell_value(r, 3)
    bloq = sh.cell_value(r, 4)
    partner = sh.cell_value(r, 7) if sh.ncols > 7 else ''
    usuarios = sh.cell_value(r, 8) if sh.ncols > 8 else ''
    print(f"{code:<8} {str(nombre)[:30]:<30} {prov!s:<10} {producto:<10} {bloq:<5} {str(partner)[:15]:<15} {usuarios}")

# Also check Potencial_Core for 1123 neighbourhood
print("\n\n=== Codigos en Potencial_Core alrededor de 1123 ===")
sh2 = wb.sheet_by_name('Potencial_Core')
for r in range(1, sh2.nrows):
    code = sh2.cell_value(r, 0)
    if isinstance(code, float) and 1120 <= int(code) <= 1130:
        print(f"  Fila {r+1}: code={int(code)}, name={sh2.cell_value(r, 1)}, short={sh2.cell_value(r, 2)}")
