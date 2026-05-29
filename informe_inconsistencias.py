import json
import xlrd
from collections import Counter

data = json.load(open('Data/seed-data.json', 'r', encoding='utf-8'))
wb = xlrd.open_workbook('2605_potencial_core.xls')

clients_by_code = {c['clientCode']: c for c in data['clients']}

lines = []
def out(text=""):
    lines.append(text)

out("=" * 120)
out("INFORME COMPLETO DE INCONSISTENCIAS - 2605_potencial_core.xls")
out("Generado para revisión manual y corrección")
out("=" * 120)

# =====================================================================
# 1. hasX flags vs pestaña individual
# =====================================================================
out("\n")
out("█" * 120)
out("  1. POTENCIAL_CORE DICE 'NO TIENE PRODUCTO' PERO SÍ APARECE EN LA PESTAÑA DEL PRODUCTO")
out("█" * 120)
out("")
out(f"{'Código':<8} {'Cliente':<50} {'Flag en Potencial_Core':<25} {'Pestaña donde SÍ aparece'}")
out("-" * 120)

field_map = {
    'hasMobileA': 'MobileA',
    'hasMobileE': 'MobileE',
    'hasAuna': 'Auna',
    'hasFegime': 'Fegime',
    'hasObras': 'Obras'
}

sats_by_code_product = set()
for s in data['satellites']:
    sats_by_code_product.add((s['clientCode'], s['productName']))

for c in sorted(data['clients'], key=lambda x: x['clientCode']):
    code = c['clientCode']
    for field, product in field_map.items():
        flag_says_yes = c.get(field, False)
        has_record = (code, product) in sats_by_code_product
        if not flag_says_yes and has_record:
            out(f"{code:<8} {c['businessName'][:50]:<50} {field}=NO{'':<16} Pestaña {product}")
        elif flag_says_yes and not has_record:
            out(f"{code:<8} {c['businessName'][:50]:<50} {field}=SI{'':<16} NO está en pestaña {product}")

# =====================================================================
# 2. Códigos huérfanos
# =====================================================================
out("\n\n")
out("█" * 120)
out("  2. CLIENTES EN PESTAÑAS SATÉLITE QUE NO EXISTEN EN POTENCIAL_CORE (huérfanos)")
out("     Estos clientes NO se cargan en la app porque no tienen ficha principal.")
out("█" * 120)

# Read names from sheets
sheet_client_names = {}
for sheet_name in ['GO!Manage', 'MobileA', 'MobileE', 'Fegime', 'Auna', 'Obras']:
    try:
        sh = wb.sheet_by_name(sheet_name)
        for r in range(1, sh.nrows):
            code_raw = sh.cell_value(r, 0)
            if code_raw:
                code = int(code_raw)
                name = str(sh.cell_value(r, 1)).strip()
                if code not in sheet_client_names and name:
                    sheet_client_names[code] = name
    except:
        pass

orphan_details = {}
for s in data['satellites']:
    code = s['clientCode']
    if code not in clients_by_code:
        if code not in orphan_details:
            orphan_details[code] = {'name': sheet_client_names.get(code, '???'), 'products': [], 'users': {}}
        if s['productName'] not in orphan_details[code]['products']:
            orphan_details[code]['products'].append(s['productName'])
        orphan_details[code]['users'][s['productName']] = s['usersProduct']

out("")
out(f"Total: {len(orphan_details)} clientes huérfanos")
out("")
out(f"{'Código':<8} {'Nombre (de pestaña satélite)':<45} {'Productos':<35} {'Usuarios por producto'}")
out("-" * 120)
for code in sorted(orphan_details.keys()):
    d = orphan_details[code]
    prods = ", ".join(d['products'])
    users_str = ", ".join(f"{p}:{d['users'][p]}" for p in d['products'])
    out(f"{code:<8} {d['name'][:45]:<45} {prods:<35} {users_str}")

# =====================================================================
# 3. Duplicados
# =====================================================================
out("\n\n")
out("█" * 120)
out("  3. REGISTROS DUPLICADOS (mismo código + mismo producto aparece más de una vez)")
out("█" * 120)

sat_keys = [(s['clientCode'], s['productName']) for s in data['satellites']]
dupes = [(k, v) for k, v in Counter(sat_keys).items() if v > 1]
dupes.sort(key=lambda x: (x[0][1], x[0][0]))

out("")
out(f"Total: {len(dupes)} combinaciones duplicadas")
out("")
out(f"{'Código':<8} {'Cliente':<45} {'Producto':<12} {'Veces':<6} {'Detalle usuarios'}")
out("-" * 120)

for (code, product), count in dupes:
    name = clients_by_code.get(code, {}).get('businessName', sheet_client_names.get(code, '???'))
    # Get user counts for each duplicate
    user_vals = [s['usersProduct'] for s in data['satellites'] if s['clientCode'] == code and s['productName'] == product]
    user_detail = f"usuarios: {user_vals}"
    out(f"{code:<8} {name[:45]:<45} {product:<12} {count:<6} {user_detail}")

# =====================================================================
# 4. Usuarios diferentes
# =====================================================================
out("\n\n")
out("█" * 120)
out("  4. USUARIOS ERP DISTINTOS ENTRE POTENCIAL_CORE Y PESTAÑA GO!MANAGE")
out("     ¿Cuál es el correcto?")
out("█" * 120)

gomanage_users = {}
for s in data['satellites']:
    if s['productName'] == 'GO!Manage':
        code = s['clientCode']
        if code not in gomanage_users:
            gomanage_users[code] = s['usersProduct']

out("")
out(f"{'Código':<8} {'Cliente':<45} {'Usrs Potencial_Core':<20} {'Usrs GO!Manage':<15} {'Diferencia'}")
out("-" * 120)

mismatches = []
for c in data['clients']:
    code = c['clientCode']
    if code in gomanage_users and c['usersErp'] != gomanage_users[code]:
        diff = c['usersErp'] - gomanage_users[code]
        mismatches.append((code, c['businessName'], c['usersErp'], gomanage_users[code], diff))

mismatches.sort(key=lambda x: -abs(x[4]))
for code, name, u1, u2, diff in mismatches:
    sign = "+" if diff > 0 else ""
    out(f"{code:<8} {name[:45]:<45} {u1:<20} {u2:<15} {sign}{diff}")

# =====================================================================
# 5. Clientes solo en una pestaña
# =====================================================================
out("\n\n")
out("█" * 120)
out("  5a. CLIENTES EN POTENCIAL_CORE QUE NO APARECEN EN GO!MANAGE")
out("      (¿deberían estar? ¿son bajas?)")
out("█" * 120)

gomanage_codes = set(s['clientCode'] for s in data['satellites'] if s['productName'] == 'GO!Manage')
main_codes = set(c['clientCode'] for c in data['clients'])
in_main_not_gm = sorted(main_codes - gomanage_codes)

out("")
out(f"Total: {len(in_main_not_gm)} clientes")
out("")
out(f"{'Código':<8} {'Cliente':<50} {'Usrs ERP':<10} {'ARR Actual':<12} {'Es DAM'}")
out("-" * 120)
for code in in_main_not_gm:
    c = clients_by_code[code]
    out(f"{code:<8} {c['businessName'][:50]:<50} {c['usersErp']:<10} {c['arrActual']:<12.0f} {'SÍ' if c['isDam'] else 'NO'}")

out("\n\n")
out("█" * 120)
out("  5b. CLIENTES EN GO!MANAGE QUE NO APARECEN EN POTENCIAL_CORE")
out("      (¿faltan en la pestaña principal? ¿son clientes nuevos o dados de baja?)")
out("█" * 120)

in_gm_not_main = sorted(gomanage_codes - main_codes)
out("")
out(f"Total: {len(in_gm_not_main)} clientes")
out("")

# Get details from GO!Manage sheet
gm_sheet = wb.sheet_by_name('GO!Manage')
gm_headers = [gm_sheet.cell_value(0, c) for c in range(gm_sheet.ncols)]
gm_by_code = {}
for r in range(1, gm_sheet.nrows):
    code_raw = gm_sheet.cell_value(r, 0)
    if code_raw:
        code = int(code_raw)
        gm_by_code[code] = {
            'name': str(gm_sheet.cell_value(r, 1)).strip(),
            'province': gm_sheet.cell_value(r, 2),
            'product': str(gm_sheet.cell_value(r, 3)).strip(),
            'blocked': str(gm_sheet.cell_value(r, 4)).strip(),
            'partner': str(gm_sheet.cell_value(r, 7)).strip() if gm_sheet.ncols > 7 else '',
            'users': int(gm_sheet.cell_value(r, 8)) if gm_sheet.ncols > 8 and gm_sheet.cell_value(r, 8) else 0
        }

out(f"{'Código':<8} {'Nombre (en GO!Manage)':<45} {'Producto':<15} {'Bloq':<6} {'Partner':<20} {'Usuarios'}")
out("-" * 120)
for code in in_gm_not_main:
    if code in gm_by_code:
        d = gm_by_code[code]
        out(f"{code:<8} {d['name'][:45]:<45} {d['product']:<15} {d['blocked']:<6} {d['partner'][:20]:<20} {d['users']}")
    else:
        out(f"{code:<8} {'(sin datos)':<45}")

# =====================================================================
# 7. Partner con zona vs sin zona
# =====================================================================
out("\n\n")
out("█" * 120)
out("  6. PARTNER DIFERENTE ENTRE POTENCIAL_CORE Y PESTAÑAS SATÉLITE")
out("     (Potencial_Core pone nombre genérico, satélites ponen nombre con zona)")
out("█" * 120)

partner_diffs = {}
for s in data['satellites']:
    code = s['clientCode']
    if code in clients_by_code:
        main_partner = clients_by_code[code]['partner']
        sat_partner = s.get('partner', '')
        if sat_partner and main_partner and sat_partner.strip().upper() != main_partner.strip().upper():
            if code not in partner_diffs:
                partner_diffs[code] = {
                    'name': clients_by_code[code]['businessName'],
                    'mainPartner': main_partner,
                    'satPartners': set()
                }
            partner_diffs[code]['satPartners'].add(sat_partner)

out("")
out(f"Total: {len(partner_diffs)} clientes con partner diferente")
out("")
out(f"{'Código':<8} {'Cliente':<40} {'Partner Potencial_Core':<22} {'Partner en Satélites'}")
out("-" * 120)
for code in sorted(partner_diffs.keys()):
    d = partner_diffs[code]
    sat_p = " / ".join(sorted(d['satPartners']))
    out(f"{code:<8} {d['name'][:40]:<40} {d['mainPartner']:<22} {sat_p}")

# =====================================================================
# Write file
# =====================================================================
out("\n\n" + "=" * 120)
out("FIN DEL INFORME")
out("=" * 120)

report = "\n".join(lines)
with open('INFORME_INCONSISTENCIAS.txt', 'w', encoding='utf-8') as f:
    f.write(report)

print(f"Informe generado: INFORME_INCONSISTENCIAS.txt ({len(lines)} líneas)")
