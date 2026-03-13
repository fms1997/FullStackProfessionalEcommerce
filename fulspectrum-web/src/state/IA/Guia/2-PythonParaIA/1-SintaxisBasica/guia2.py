# 1️⃣ Diccionarios
# 2️⃣ Tuplas
# 3️⃣ Sets
# 4️⃣ List Comprehensions
# 5️⃣ Manejo de archivos (CSV / texto)
# 6️⃣ Introducción a NumPy (base de la IA)

# 1️⃣ Diccionarios (MUY usados en IA)

# Un diccionario es una estructura clave → valor.

# Es como un JSON.

# persona = {
#     "nombre": "Franco",
#     "edad": 30,
#     "pais": "Argentina"
# }
# Acceder a valores
# print(persona["nombre"])   # Franco
# print(persona["edad"])     # 30
# Modificar valores
# persona["edad"] = 31
# Agregar campos
# persona["profesion"] = "programador"
# Eliminar
# del persona["pais"]
# Recorrer diccionarios
# for clave, valor in persona.items():
#     print(clave, valor)
# Métodos importantes
# persona.keys()      # claves
# persona.values()    # valores
# persona.items()     # pares clave-valor
# Ejemplo IA: contar clases
# labels = ["gato", "perro", "gato", "pajaro"]

# conteo = {}

# for l in labels:
#     conteo[l] = conteo.get(l, 0) + 1

# print(conteo)

# Resultado:

# {'gato': 2, 'perro': 1, 'pajaro': 1}

# Esto es muy común en machine learning.

# 2️⃣ Tuplas

# Las tuplas son como listas pero inmutables.

# punto = (10, 20)

# No se pueden modificar.

# punto[0] = 30   # ERROR
# Acceso
# x = punto[0]
# y = punto[1]
# Desempaquetado

# Muy usado en Python:

# x, y = punto
# Ejemplo real
# def min_max(lista):
#     return min(lista), max(lista)

# mn, mx = min_max([1,5,10])
# 3️⃣ Sets (conjuntos)

# Los sets no permiten duplicados.

# nums = {1,2,3,3,4}

# print(nums)

# Resultado:

# {1,2,3,4}
# Usos en IA

# Eliminar duplicados.

# palabras = ["hola", "mundo", "hola"]

# unicas = set(palabras)

# print(unicas)
# Operaciones matemáticas
# A = {1,2,3}
# B = {3,4,5}

# A.union(B)
# A.intersection(B)
# A.difference(B)
# 4️⃣ List Comprehension (MUY importante)

# Forma compacta de crear listas.

# Forma normal
# cuadrados = []

# for i in range(5):
#     cuadrados.append(i*i)
# Forma Pythonica
# cuadrados = [i*i for i in range(5)]

# Resultado:

# [0,1,4,9,16]
# Con condición
# pares = [x for x in range(10) if x % 2 == 0]

# Resultado:

# [0,2,4,6,8]
# Ejemplo IA

# Normalizar valores:

# datos = [10,20,30]

# norm = [x/30 for x in datos]
# 5️⃣ Manejo de archivos

# Muy usado para datasets.

# Leer archivo de texto

# Archivo datos.txt

# 10
# 20
# 30
# 40

# Código:

# with open("datos.txt", "r") as f:
#     lineas = f.readlines()

# print(lineas)
# Convertir a números
# numeros = [int(x.strip()) for x in lineas]
# Escribir archivo
# with open("salida.txt", "w") as f:
#     f.write("hola mundo")
# 6️⃣ Leer CSV (datasets)

# Archivo datos.csv

# edad,salario
# 25,2000
# 30,3000
# 40,5000

# Código:

# import csv

# with open("datos.csv") as f:
#     reader = csv.reader(f)
    
#     for fila in reader:
#         print(fila)

# Resultado:

# ['edad', 'salario']
# ['25', '2000']
# ['30', '3000']
# 7️⃣ Introducción a NumPy (BASE de IA)

# NumPy es la base matemática de machine learning.

# Instalar:

# pip install numpy
# Importar
# import numpy as np
# Crear array
# a = np.array([1,2,3,4])
# Operaciones vectorizadas

# Esto es lo poderoso:

# a * 2

# Resultado

# [2 4 6 8]
# Suma
# np.sum(a)
# Promedio
# np.mean(a)
# Matrices
# m = np.array([
#     [1,2,3],
#     [4,5,6]
# ])
# Shape
# m.shape

# Resultado:

# (2,3)
# Indexado
# m[0,1]   # 2