import json

data = json.load(open('Data/seed-data.json', 'r', encoding='utf-8'))

client_codes = set(c['clientCode'] for c in data['clients'])
obras = [s for s in data['satellites'] if s['productName'] == 'Obras']

print(f"Total registros Obras en seed: {len(obras)}")
print(f"Codigos unicos Obras: {len(set(s['clientCode'] for s in obras))}")
print()

in_clients = []
not_in_clients = []
for s in obras:
    code = s['clientCode']
    if code in client_codes:
        in_clients.append(code)
    else:
        not_in_clients.append(code)

in_clients_unique = sorted(set(in_clients))
not_in_clients_unique = sorted(set(not_in_clients))

print(f"Obras CON cliente en tabla clients ({len(in_clients_unique)}): {in_clients_unique}")
print(f"Obras SIN cliente en tabla clients ({len(not_in_clients_unique)}): {not_in_clients_unique}")
print(f"\n1123 esta en la lista que SI pasa el filtro: {1123 in in_clients_unique}")
