# 1) Variables y tipos básicos
# 1.1 Asignación de variables

# En Python no declarás tipos explícitos:

# nombre = "Franco"
# edad = 30
# altura = 1.78
# es_activo = True
# 1.2 Tipos más usados

# int: enteros

# float: decimales

# str: texto

# bool: True/False

# None: “nada” (muy usado para inicializar)

# x = 10          # int
# y = 3.14        # float
# s = "hola"      # str
# ok = False      # bool
# z = None        # NoneType
# 1.3 Conversión de tipos

# Muy común cuando leés datos (CSV, input, JSON):

# a = "123"
# b = int(a)      # 123
# c = float("3.5")# 3.5
# d = str(999)    # "999"
# 1.4 Operadores básicos
# # aritmética
# 1 + 2
# 5 - 3
# 4 * 2
# 10 / 4      # 2.5 (siempre float)
# 10 // 4     # 2 (división entera)
# 10 % 4      # 2 (resto)
# 2 ** 3      # 8 (potencia)

# # comparación
# 3 > 2
# 3 == 3
# 3 != 4

# # lógicos
# True and False
# True or False
# not True
# 2) Listas (clave para IA)

# En IA vas a vivir entre listas, arrays y tensores. La lista es el inicio.

# 2.1 Crear listas
# numeros = [1, 2, 3, 4]
# nombres = ["ana", "leo", "sol"]
# mixta = [1, "hola", True, 3.14]
# vacia = []
# 2.2 Acceso por índice

# Python usa índice desde 0:

# numeros[0]   # 1
# numeros[2]   # 3
# numeros[-1]  # último elemento -> 4
# 2.3 Slicing (rebanado)
# numeros[1:3]   # [2, 3]
# numeros[:2]    # [1, 2]
# numeros[2:]    # [3, 4]
# numeros[::2]   # [1, 3] (salto de 2)
# 2.4 Modificar listas
# numeros[1] = 99         # [1, 99, 3, 4]
# numeros.append(5)       # agrega al final
# numeros.insert(1, 77)   # inserta en posición 1
# numeros.remove(99)      # elimina por valor (primera coincidencia)
# ultimo = numeros.pop()  # saca y devuelve el último
# 2.5 Funciones útiles de listas
# len(numeros)       # cantidad
# sum([1,2,3])       # suma
# min(numeros)       # mínimo
# max(numeros)       # máximo
# sorted(numeros)    # devuelve lista ordenada (sin modificar original)
# numeros.sort()     # ordena en el lugar (modifica)
# 3) Bucles (loops) y control de flujo
# 3.1 if / elif / else
# x = 10

# if x > 10:
#     print("mayor")
# elif x == 10:
#     print("igual")
# else:
#     print("menor")
# 3.2 for (lo más común)

# Recorrer lista:

# datos = [10, 20, 30]
# for d in datos:
#     print(d)

# Con índice (muy usado para datasets):

# for i, d in enumerate(datos):
#     print(i, d)

# Con rangos:

# for i in range(5):      # 0..4
#     print(i)

# for i in range(2, 10, 2):  # 2,4,6,8
#     print(i)
# 3.3 while

# Se usa cuando no sabés cuántas iteraciones:

# i = 0
# while i < 3:
#     print(i)
#     i += 1
# 3.4 break y continue
# for x in [1,2,3,4,5]:
#     if x == 3:
#         continue  # salta esta vuelta
#     if x == 5:
#         break     # corta el loop
#     print(x)
# 4) Funciones (fundamentales para ordenar código)
# 4.1 Definir y usar
# def saludar(nombre):
#     return f"Hola, {nombre}"

# print(saludar("Franco"))
# 4.2 Parámetros con valor por defecto
# def potencia(base, exponente=2):
#     return base ** exponente

# potencia(3)    # 9
# potencia(3, 3) # 27
# 4.3 Devolver múltiples valores

# Python devuelve una tupla:

# def estadisticas(nums):
#     return min(nums), max(nums), sum(nums)/len(nums)

# mn, mx, prom = estadisticas([1, 2, 10])
# 4.4 *args y **kwargs (muy común en librerías de IA)

# *args: argumentos posicionales variables

# **kwargs: argumentos con nombre variables

# def sumar_todo(*args):
#     return sum(args)

# sumar_todo(1, 2, 3, 4)  # 10


# def configurar_modelo(**kwargs):
#     # kwargs es un dict
#     return kwargs

# configurar_modelo(lr=0.001, epochs=10, batch_size=32)
# 5) Mini-ejercicios orientados a IA (práctica real)
# Ejercicio A: normalizar datos (0 a 1)
# def normalizar(lista):
#     mn = min(lista)
#     mx = max(lista)
#     return [(x - mn) / (mx - mn) for x in lista]

# print(normalizar([10, 20, 30]))
# Ejercicio B: contar etiquetas (como clases)
# def contar_etiquetas(labels):
#     conteo = {}
#     for y in labels:
#         conteo[y] = conteo.get(y, 0) + 1
#     return conteo

# print(contar_etiquetas(["spam", "ham", "spam"]))
# Ejercicio C: separar train/test (simple)
# def split_train_test(data, ratio=0.8):
#     n_train = int(len(data) * ratio)
#     return data[:n_train], data[n_train:]

# train, test = split_train_test([1,2,3,4,5], 0.6)
# print(train, test)







