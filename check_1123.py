import json
import xlrd

# 1. Check in seed-data.json
data = json.load(open('Data/seed-data.json', 'r', encoding='utf-8'))

obras = [s for s in data['satellites'] if 'obra' in s.get('productName','').lower()]
print(f"=== SEED DATA ===")
print(f"Total registros Obras en seed: {len(obras)}")

obras_codes = sorted(set(s['clientCode'] for s in obras))
print(f"Codigos en Obras (seed): {obras_codes}")
print(f"1123 en Obras (seed): {1123 in obras_codes}")

all_client_codes = sorted(set(c['clientCode'] for c in data['clients']))
print(f"\n1123 en tabla clients (seed): {1123 in all_client_codes}")

# Check codes close to 1123
close = [c for c in all_client_codes if abs(c - 1123) < 10]
print(f"Codigos cercanos a 1123 en clients: {close}")

close_obras = [c for c in obras_codes if abs(c - 1123) < 10]
print(f"Codigos cercanos a 1123 en Obras: {close_obras}")

# 2. Check in original Excel
print(f"\n=== EXCEL ORIGINAL ===")
wb = xlrd.open_workbook('2605_potencial_core.xls')
sh = wb.sheet_by_name('Obras')
print(f"Columnas: {[sh.cell_value(0, c) for c in range(sh.ncols)]}")
print(f"Total filas (sin cabecera): {sh.nrows - 1}")

found = False
for r in range(1, sh.nrows):
    code = sh.cell_value(r, 0)
    if isinstance(code, float):
        code_int = int(code)
    else:
        code_int = None
    if code_int == 1123:
        print(f"ENCONTRADO en Excel Obras fila {r+1}: {[sh.cell_value(r, c) for c in range(sh.ncols)]}")
        found = True

if not found:
    print("1123 NO encontrado en hoja Obras del Excel")
    # Check all sheets for 1123
    print(f"\nBuscando 1123 en TODAS las hojas:")
    for name in wb.sheet_names():
        s = wb.sheet_by_name(name)
        for r in range(1, s.nrows):
            code = s.cell_value(r, 0)
            if isinstance(code, float) and int(code) == 1123:
                print(f"  -> Encontrado en hoja '{name}' fila {r+1}: {[s.cell_value(r, c) for c in range(min(5, s.ncols))]}")

# 3. Check column mapping in generate script
print(f"\n=== POTENCIAL_CORE (hoja principal) ===")
sh2 = wb.sheet_by_name('Potencial_Core')
for r in range(1, sh2.nrows):
    code = sh2.cell_value(r, 0)
    if isinstance(code, float) and int(code) == 1123:
        print(f"Cliente 1123 en Potencial_Core fila {r+1}: {[sh2.cell_value(r, c) for c in range(sh2.ncols)]}")
        break
