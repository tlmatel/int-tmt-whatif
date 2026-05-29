import json
from collections import defaultdict

data = json.load(open('Data/seed-data.json', 'r', encoding='utf-8'))
clients = data['clients']
satellites = data['satellites']
client_codes = set(c['clientCode'] for c in clients)
client_map = {c['clientCode']: c for c in clients}

valid_sats = [s for s in satellites if s['clientCode'] in client_codes]

products = defaultdict(lambda: {'clients': {}, 'total_users': 0})
for s in valid_sats:
    p = s['productName']
    cc = s['clientCode']
    if cc not in products[p]['clients']:
        products[p]['clients'][cc] = {
            'sat_users': 0,
            'code': cc,
            'name': client_map[cc]['businessName'],
            'erp_users': client_map[cc]['usersErp'],
            'arr_actual': client_map[cc]['arrActual'],
            'partner': client_map[cc]['partner'],
            'city': client_map[cc]['city'],
        }
    products[p]['clients'][cc]['sat_users'] += s['usersProduct']
    products[p]['total_users'] += s['usersProduct']

print("=" * 80)
print("ANÁLISIS DE RUTA DE MIGRACIÓN DE SATÉLITES")
print("=" * 80)

for p in ['MobileA', 'MobileE', 'Obras', 'Fegime', 'Auna']:
    info = products[p]
    n_clients = len(info['clients'])
    total_users = info['total_users']
    sorted_clients = sorted(info['clients'].values(), key=lambda x: -x['sat_users'])
    
    print(f"\n{'='*60}")
    print(f"  {p} — {n_clients} clientes, {total_users} usuarios satélite")
    print(f"{'='*60}")
    print(f"  {'Cliente':<35} {'Ciudad':<15} {'ERP':>4} {'SAT':>4} {'ARR Actual':>10}")
    print(f"  {'-'*35} {'-'*15} {'-'*4} {'-'*4} {'-'*10}")
    for c in sorted_clients:
        print(f"  {c['name'][:35]:<35} {c['city'][:15]:<15} {c['erp_users']:>4} {c['sat_users']:>4} {c['arr_actual']:>10,.0f}")

print("\n\n")
print("=" * 80)
print("JSON para la vista (copiar):")
print("=" * 80)

import json as j
output = {}
for p in ['MobileA', 'MobileE', 'Obras', 'Fegime', 'Auna']:
    info = products[p]
    sorted_clients = sorted(info['clients'].values(), key=lambda x: -x['sat_users'])
    output[p] = {
        'totalClients': len(info['clients']),
        'totalUsers': info['total_users'],
        'clients': [{'code': c['code'], 'name': c['name'], 'city': c['city'], 'erpUsers': c['erp_users'], 'satUsers': c['sat_users'], 'arrActual': c['arr_actual'], 'partner': c['partner']} for c in sorted_clients]
    }

print(j.dumps(output, ensure_ascii=False, indent=2))
