import json
import xlrd

data = json.load(open('Data/seed-data.json', 'r', encoding='utf-8'))
wb = xlrd.open_workbook('2605_potencial_core.xls')

# Build lookup from Potencial_Core
clients_by_code = {c['clientCode']: c for c in data['clients']}

# Build lookup of satellites by clientCode+product
sats_by_code = {}
for s in data['satellites']:
    key = (s['clientCode'], s['productName'])
    sats_by_code[key] = s

print("=" * 100)
print("INFORME DE INCONSISTENCIAS - 2605_potencial_core.xls")
print("=" * 100)

# 1. Check has* flags vs actual satellite records
print("\n\n### 1. INCONSISTENCIA: hasX en Potencial_Core vs existencia en pestaña individual ###\n")
field_map = {
    'hasMobileA': 'MobileA',
    'hasMobileE': 'MobileE',
    'hasAuna': 'Auna',
    'hasFegime': 'Fegime',
    'hasObras': 'Obras'
}

inconsistencies_flags = []
for c in data['clients']:
    code = c['clientCode']
    for field, product in field_map.items():
        flag_says_yes = c.get(field, False)
        has_satellite_record = (code, product) in sats_by_code
        
        if flag_says_yes and not has_satellite_record:
            inconsistencies_flags.append({
                'code': code, 'name': c['businessName'],
                'type': f"Potencial_Core dice {field}=SI pero NO existe en pestaña {product}"
            })
        elif not flag_says_yes and has_satellite_record:
            inconsistencies_flags.append({
                'code': code, 'name': c['businessName'],
                'type': f"Potencial_Core dice {field}=NO pero SI existe en pestaña {product}"
            })

print(f"Total: {len(inconsistencies_flags)} inconsistencias")
for i in inconsistencies_flags:
    print(f"  Código {i['code']:>5} | {i['name'][:40]:<40} | {i['type']}")


# 2. Satellite records without matching client
print("\n\n### 2. CÓDIGOS EN PESTAÑAS SATÉLITE QUE NO EXISTEN EN POTENCIAL_CORE ###\n")
orphans = []
for s in data['satellites']:
    if s['clientCode'] not in clients_by_code:
        orphans.append(s)

orphan_by_product = {}
for o in orphans:
    orphan_by_product.setdefault(o['productName'], []).append(o['clientCode'])

if orphans:
    print(f"Total: {len(orphans)} registros huérfanos")
    for product, codes in sorted(orphan_by_product.items()):
        print(f"  {product}: códigos {sorted(set(codes))}")
else:
    print("Ninguno.")


# 3. Duplicate satellite records (same code+product)
print("\n\n### 3. REGISTROS DUPLICADOS EN PESTAÑAS SATÉLITE ###\n")
from collections import Counter
sat_keys = [(s['clientCode'], s['productName']) for s in data['satellites']]
dupes = [(k, v) for k, v in Counter(sat_keys).items() if v > 1]
if dupes:
    print(f"Total: {len(dupes)} duplicados")
    for (code, product), count in dupes:
        name = clients_by_code.get(code, {}).get('businessName', '???')
        print(f"  Código {code} | {name[:40]} | {product} aparece {count} veces")
else:
    print("Ninguno.")


# 4. Users mismatch: Potencial_Core usersErp vs GO!Manage sheet users
print("\n\n### 4. USUARIOS ERP (Potencial_Core) vs USUARIOS GO!MANAGE (pestaña GO!Manage) ###\n")
gomanage_sats = {s['clientCode']: s['usersProduct'] for s in data['satellites'] if s['productName'] == 'GO!Manage'}
user_mismatches = []
for c in data['clients']:
    code = c['clientCode']
    users_main = c['usersErp']
    users_gomanage = gomanage_sats.get(code)
    if users_gomanage is not None and users_main != users_gomanage:
        user_mismatches.append({
            'code': code, 'name': c['businessName'],
            'usersMain': users_main, 'usersGM': users_gomanage
        })

if user_mismatches:
    print(f"Total: {len(user_mismatches)} diferencias")
    for m in user_mismatches[:30]:
        print(f"  Código {m['code']:>5} | {m['name'][:35]:<35} | Potencial_Core: {m['usersMain']:>3} | GO!Manage: {m['usersGM']:>3}")
    if len(user_mismatches) > 30:
        print(f"  ... y {len(user_mismatches) - 30} más")
else:
    print("Todos coinciden.")


# 5. Clients in Potencial_Core NOT in GO!Manage and vice versa
print("\n\n### 5. CLIENTES QUE ESTÁN EN UNA PESTAÑA PERO NO EN OTRA ###\n")
gomanage_codes = set(s['clientCode'] for s in data['satellites'] if s['productName'] == 'GO!Manage')
main_codes = set(c['clientCode'] for c in data['clients'])

in_main_not_gomanage = main_codes - gomanage_codes
in_gomanage_not_main = gomanage_codes - main_codes

print(f"En Potencial_Core pero NO en GO!Manage: {len(in_main_not_gomanage)}")
if in_main_not_gomanage:
    for code in sorted(in_main_not_gomanage)[:20]:
        name = clients_by_code[code]['businessName']
        print(f"  Código {code:>5} | {name[:50]}")
    if len(in_main_not_gomanage) > 20:
        print(f"  ... y {len(in_main_not_gomanage) - 20} más")

print(f"\nEn GO!Manage pero NO en Potencial_Core: {len(in_gomanage_not_main)}")
if in_gomanage_not_main:
    for code in sorted(in_gomanage_not_main):
        print(f"  Código {code:>5}")


# 6. Name differences between sheets
print("\n\n### 6. NOMBRES DIFERENTES ENTRE PESTAÑAS (comparando con Potencial_Core) ###\n")
# Read names from individual sheets
sheet_names_map = {
    'GO!Manage': 'GO!Manage',
    'MobileA': 'MobileA', 
    'MobileE': 'MobileE',
    'Fegime': 'Fegime',
    'Auna': 'Auna',
    'Obras': 'Obras'
}

name_diffs = []
for sheet_key, product in sheet_names_map.items():
    try:
        sh = wb.sheet_by_name(sheet_key)
    except:
        continue
    
    for r in range(1, sh.nrows):
        code_raw = sh.cell_value(r, 0)
        if not code_raw:
            continue
        code = int(code_raw)
        sheet_name = str(sh.cell_value(r, 1)).strip()
        
        if code in clients_by_code:
            main_name = clients_by_code[code]['businessName']
            short_name = clients_by_code[code].get('name', '')
            if sheet_name and sheet_name != main_name and sheet_name != short_name:
                name_diffs.append({
                    'code': code,
                    'sheet': product,
                    'nameInSheet': sheet_name,
                    'nameInMain': main_name,
                    'shortName': short_name
                })

if name_diffs:
    print(f"Total: {len(name_diffs)} nombres diferentes")
    for d in name_diffs[:40]:
        print(f"  Código {d['code']:>5} | Pestaña {d['sheet']:<10} | En pestaña: '{d['nameInSheet'][:30]}' | En Potencial_Core: '{d['shortName'][:30]}' / '{d['nameInMain'][:30]}'")
    if len(name_diffs) > 40:
        print(f"  ... y {len(name_diffs) - 40} más")
else:
    print("Todos los nombres coinciden.")


# 7. Partner differences
print("\n\n### 7. PARTNER DIFERENTE ENTRE PESTAÑAS ###\n")
partner_diffs = []
for s in data['satellites']:
    code = s['clientCode']
    if code in clients_by_code:
        main_partner = clients_by_code[code]['partner']
        sat_partner = s.get('partner', '')
        if sat_partner and main_partner and sat_partner.strip().upper() != main_partner.strip().upper():
            partner_diffs.append({
                'code': code,
                'product': s['productName'],
                'partnerMain': main_partner,
                'partnerSat': sat_partner
            })

unique_partner_diffs = {}
for p in partner_diffs:
    key = (p['code'], p['partnerMain'], p['partnerSat'])
    if key not in unique_partner_diffs:
        unique_partner_diffs[key] = p

if unique_partner_diffs:
    print(f"Total: {len(unique_partner_diffs)} clientes con partner diferente")
    for p in list(unique_partner_diffs.values())[:20]:
        name = clients_by_code[p['code']]['businessName'][:30]
        print(f"  Código {p['code']:>5} | {name:<30} | Main: {p['partnerMain']:<20} | Satélite: {p['partnerSat']}")
    if len(unique_partner_diffs) > 20:
        print(f"  ... y {len(unique_partner_diffs) - 20} más")
else:
    print("Todos coinciden.")

print("\n\n" + "=" * 100)
print("FIN DEL INFORME")
print("=" * 100)
