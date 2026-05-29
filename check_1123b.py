import json

data = json.load(open('Data/seed-data.json', 'r', encoding='utf-8'))

# Check all satellite entries for client 1123
sats_1123 = [s for s in data['satellites'] if s['clientCode'] == 1123]
print(f"Satelites del cliente 1123 en seed-data.json:")
for s in sats_1123:
    print(f"  productName='{s['productName']}', users={s['usersProduct']}")

# Check what productName is used for Obras entries
obras_names = set(s['productName'] for s in data['satellites'] if 'obra' in s.get('productName','').lower())
print(f"\nNombres de producto para Obras: {obras_names}")

# Check all distinct productNames
all_names = sorted(set(s['productName'] for s in data['satellites']))
print(f"\nTodos los productName distintos en seed: {all_names}")

# Also check the DbSeeder filtering - does client 1123 exist in clients?
client_1123 = [c for c in data['clients'] if c['clientCode'] == 1123]
print(f"\nCliente 1123 en clients: {client_1123[0]['businessName'] if client_1123 else 'NO ENCONTRADO'}")
