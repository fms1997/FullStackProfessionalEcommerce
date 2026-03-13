# Arrays y operaciones numéricas
# 1. ¿Qué es NumPy y por qué es clave en IA?

# NumPy es una librería de Python para trabajar con:

# arrays multidimensionales

# operaciones matemáticas rápidas

# álgebra lineal

# estadísticas

# manipulación de datos numéricos

# En IA se usa porque:

# los datos suelen representarse como vectores, matrices o tensores

# los modelos trabajan con muchas operaciones numéricas

# NumPy es mucho más rápido que listas normales de Python

# muchas librerías de IA se apoyan en ideas de NumPy

# Ejemplos en IA:

# una imagen puede ser una matriz de píxeles

# un dataset puede ser una matriz donde cada fila es una muestra

# los pesos de una red neuronal pueden ser matrices

# una predicción puede ser el resultado de multiplicaciones y sumas entre arrays

# 2. Instalación e importación

# Si no lo tenés instalado:

# pip install numpy

# Importación estándar:

# import numpy as np

# Se usa np por convención.

# 3. Diferencia entre lista de Python y array de NumPy
# Lista normal
# lista = [1, 2, 3, 4]
# Array de NumPy
# import numpy as np

# arr = np.array([1, 2, 3, 4])
# print(arr)

# Salida:

# [1 2 3 4]
# ¿Por qué usar arrays?

# Porque permiten:

# operaciones vectorizadas

# más velocidad

# menos memoria

# trabajar con muchas dimensiones

# Ejemplo:

# lista = [1, 2, 3]
# # lista * 2 repite la lista, no multiplica cada elemento
# print(lista * 2)

# Salida:

# [1, 2, 3, 1, 2, 3]

# Con NumPy:

# arr = np.array([1, 2, 3])
# print(arr * 2)

# Salida:

# [2 4 6]
# 4. Crear arrays
# 4.1 Desde listas
# Vector (1 dimensión)
# a = np.array([1, 2, 3, 4])
# print(a)
# Matriz (2 dimensiones)
# b = np.array([
#     [1, 2, 3],
#     [4, 5, 6]
# ])
# print(b)
# Array 3D
# c = np.array([
#     [[1, 2], [3, 4]],
#     [[5, 6], [7, 8]]
# ])
# print(c)
# 4.2 Arrays especiales
# zeros

# Crea un array lleno de ceros.

# a = np.zeros((3, 4))
# print(a)
# ones
# a = np.ones((2, 3))
# print(a)
# full
# a = np.full((2, 2), 7)
# print(a)
# eye o identidad

# Muy usada en álgebra lineal.

# a = np.eye(3)
# print(a)

# Salida:

# [[1. 0. 0.]
#  [0. 1. 0.]
#  [0. 0. 1.]]
# arange

# Como range, pero devuelve array.

# a = np.arange(0, 10, 2)
# print(a)

# Salida:

# [0 2 4 6 8]
# linspace

# Genera valores equiespaciados.

# a = np.linspace(0, 1, 5)
# print(a)

# Salida:

# [0.   0.25 0.5  0.75 1.  ]

# Muy útil en gráficos, simulaciones y matemática.

# random
# a = np.random.rand(3, 2)      # aleatorios entre 0 y 1
# b = np.random.randint(1, 10, size=(2, 3))  # enteros
# 5. Propiedades importantes de un array
# a = np.array([
#     [1, 2, 3],
#     [4, 5, 6]
# ])
# shape

# Indica dimensiones.

# print(a.shape)

# Salida:

# (2, 3)

# 2 filas, 3 columnas.

# ndim

# Número de dimensiones.

# print(a.ndim)

# Salida:

# 2
# size

# Cantidad total de elementos.

# print(a.size)

# Salida:

# 6
# dtype

# Tipo de dato.

# print(a.dtype)

# Salida típica:

# int64
# astype

# Convierte tipo.

# b = a.astype(float)
# print(b.dtype)
# 6. Indexación y slicing

# Esto es fundamental en IA porque casi siempre vas a seleccionar columnas, filas, features o subconjuntos.

# 6.1 En arrays 1D
# a = np.array([10, 20, 30, 40, 50])

# print(a[0])     # 10
# print(a[-1])    # 50
# print(a[1:4])   # [20 30 40]
# 6.2 En arrays 2D
# a = np.array([
#     [1, 2, 3],
#     [4, 5, 6],
#     [7, 8, 9]
# ])
# Elemento puntual
# print(a[0, 1])  # fila 0, columna 1

# Salida:

# 2
# Fila completa
# print(a[1])

# Salida:

# [4 5 6]
# Columna completa
# print(a[:, 1])

# Salida:

# [2 5 8]
# Submatriz
# print(a[0:2, 1:3])

# Salida:

# [[2 3]
#  [5 6]]
# 7. Modificar valores
# a = np.array([1, 2, 3, 4])
# a[0] = 100
# print(a)

# Salida:

# [100   2   3   4]

# En 2D:

# a = np.array([
#     [1, 2],
#     [3, 4]
# ])

# a[0, 1] = 99
# print(a)
# 8. Operaciones básicas entre arrays
# 8.1 Suma, resta, multiplicación, división
# a = np.array([1, 2, 3])
# b = np.array([4, 5, 6])

# print(a + b)
# print(a - b)
# print(a * b)
# print(a / b)

# Salida:

# [5 7 9]
# [-3 -3 -3]
# [ 4 10 18]
# [0.25 0.4  0.5 ]

# Esto se hace elemento por elemento.

# 8.2 Operaciones con escalares
# a = np.array([1, 2, 3])

# print(a + 10)
# print(a * 2)
# print(a / 2)
# print(a ** 2)

# Salida:

# [11 12 13]
# [2 4 6]
# [0.5 1.  1.5]
# [1 4 9]
# 9. Funciones matemáticas comunes

# NumPy incluye funciones vectorizadas.

# a = np.array([1, 4, 9, 16])

# print(np.sqrt(a))   # raíz cuadrada
# print(np.log(a))    # logaritmo natural
# print(np.exp(a))    # exponencial
# print(np.sin(a))    # seno
# print(np.cos(a))    # coseno

# En IA se usan mucho:

# np.exp() en softmax

# np.log() en pérdidas logarítmicas

# np.sqrt() en normalización

# np.maximum() para ReLU

# Ejemplo de ReLU:

# x = np.array([-2, -1, 0, 1, 2])
# relu = np.maximum(0, x)
# print(relu)

# Salida:

# [0 0 0 1 2]
# 10. Estadística básica con NumPy

# Muy importante para análisis de datos e IA.

# a = np.array([1, 2, 3, 4, 5])
# print(np.sum(a))      # suma
# print(np.mean(a))     # promedio
# print(np.min(a))      # mínimo
# print(np.max(a))      # máximo
# print(np.std(a))      # desviación estándar
# print(np.var(a))      # varianza
# 10.1 Estadística por eje (axis)

# En matrices, axis es clave.

# a = np.array([
#     [1, 2, 3],
#     [4, 5, 6]
# ])
# Suma total
# print(np.sum(a))

# Salida:

# 21
# Suma por columnas
# print(np.sum(a, axis=0))

# Salida:

# [5 7 9]
# Suma por filas
# print(np.sum(a, axis=1))

# Salida:

# [ 6 15]

# Regla práctica:

# axis=0 → baja por filas y combina columnas

# axis=1 → recorre columnas y combina filas

# 11. Comparaciones y máscaras booleanas

# Súper útil para filtrar datos.

# a = np.array([10, 20, 30, 40, 50])

# print(a > 25)

# Salida:

# [False False  True  True  True]
# Filtrar
# print(a[a > 25])

# Salida:

# [30 40 50]
# Otro ejemplo
# edades = np.array([15, 18, 22, 17, 30])
# mayores = edades[edades >= 18]
# print(mayores)

# Salida:

# [18 22 30]

# En IA esto sirve para:

# limpiar datos

# eliminar valores inválidos

# seleccionar subconjuntos

# aplicar reglas

# 12. Broadcasting

# Uno de los conceptos más importantes de NumPy.

# Permite operar arrays de diferentes formas compatibles, sin tener que duplicar datos manualmente.

# Ejemplo simple
# a = np.array([1, 2, 3])
# print(a + 10)

# El 10 se “expande” automáticamente.

# Ejemplo con matriz y vector
# a = np.array([
#     [1, 2, 3],
#     [4, 5, 6]
# ])

# b = np.array([10, 20, 30])

# print(a + b)

# Salida:

# [[11 22 33]
#  [14 25 36]]

# El vector b se suma a cada fila.

# Esto es muy usado en IA para:

# sumar bias

# normalizar columnas

# aplicar transformaciones por feature

# Reglas generales del broadcasting

# Dos dimensiones son compatibles si:

# son iguales, o

# una de ellas es 1

# Ejemplo compatible:

# (2, 3) con (3,)

# (4, 3) con (1, 3)

# (5, 1) con (5, 7)

# 13. Cambio de forma: reshape

# Muy usado para preparar datos para modelos.

# a = np.array([1, 2, 3, 4, 5, 6])

# b = a.reshape(2, 3)
# print(b)

# Salida:

# [[1 2 3]
#  [4 5 6]]

# También:

# c = a.reshape(3, 2)
# print(c)
# Con -1

# NumPy calcula automáticamente una dimensión.

# a = np.array([1, 2, 3, 4, 5, 6])
# print(a.reshape(2, -1))

# Salida:

# [[1 2 3]
#  [4 5 6]]

# En IA esto se usa para:

# convertir imágenes en vectores

# reorganizar lotes de datos

# adaptar dimensiones a modelos

# 14. Aplanar arrays
# flatten
# a = np.array([
#     [1, 2],
#     [3, 4]
# ])

# print(a.flatten())

# Salida:

# [1 2 3 4]

# Sirve para pasar de matriz a vector.

# 15. Transpuesta

# La transpuesta intercambia filas por columnas.

# a = np.array([
#     [1, 2, 3],
#     [4, 5, 6]
# ])

# print(a.T)

# Salida:

# [[1 4]
#  [2 5]
#  [3 6]]

# Muy usada en:

# álgebra lineal

# regresión lineal

# cálculo de gradientes

# ajuste de dimensiones

# 16. Concatenar y apilar arrays
# concatenate
# a = np.array([1, 2, 3])
# b = np.array([4, 5, 6])

# c = np.concatenate([a, b])
# print(c)

# Salida:

# [1 2 3 4 5 6]
# En matrices
# a = np.array([[1, 2], [3, 4]])
# b = np.array([[5, 6]])

# print(np.concatenate([a, b], axis=0))
# vstack

# Apila verticalmente.

# a = np.array([1, 2])
# b = np.array([3, 4])

# print(np.vstack([a, b]))

# Salida:

# [[1 2]
#  [3 4]]
# hstack

# Apila horizontalmente.

# print(np.hstack([a, b]))

# Salida:

# [1 2 3 4]
# 17. Producto escalar y multiplicación matricial

# Esto es central en IA.

# 17.1 Multiplicación elemento a elemento
# a = np.array([1, 2, 3])
# b = np.array([4, 5, 6])

# print(a * b)

# Salida:

# [ 4 10 18]
# 17.2 Producto escalar (dot)
# a = np.array([1, 2, 3])
# b = np.array([4, 5, 6])

# print(np.dot(a, b))

# Cálculo:

# 1*4 + 2*5 + 3*6 = 32

# Salida:

# 32
# 17.3 Multiplicación de matrices
# A = np.array([
#     [1, 2],
#     [3, 4]
# ])

# B = np.array([
#     [5, 6],
#     [7, 8]
# ])

# print(np.dot(A, B))

# O también:

# print(A @ B)

# Salida:

# [[19 22]
#  [43 50]]

# Esto se usa en IA para:

# combinar entradas con pesos

# propagación hacia adelante

# transformaciones lineales

# 18. Álgebra lineal básica con np.linalg
# Determinante
# A = np.array([
#     [1, 2],
#     [3, 4]
# ])

# print(np.linalg.det(A))
# Inversa
# print(np.linalg.inv(A))
# Valores y vectores propios
# valores, vectores = np.linalg.eig(A)
# print(valores)
# print(vectores)
# Resolver sistemas lineales
# A = np.array([[2, 1], [1, 3]])
# b = np.array([8, 13])

# x = np.linalg.solve(A, b)
# print(x)

# Esto se relaciona con optimización, regresión y reducción de dimensiones.

# 19. Números aleatorios

# Muy útil para IA: inicialización de pesos, muestreo, particiones.

# Uniforme
# a = np.random.rand(3, 3)
# print(a)
# Enteros aleatorios
# a = np.random.randint(0, 10, size=(2, 4))
# print(a)
# Distribución normal
# a = np.random.randn(3, 3)
# print(a)

# Muy usada para inicializar pesos.

# Semilla

# Para reproducibilidad:

# np.random.seed(42)
# print(np.random.rand(3))
# 20. Copia vs vista

# Esto es importante para evitar errores.

# a = np.array([1, 2, 3])
# b = a
# b[0] = 100

# print(a)

# Salida:

# [100   2   3]

# b no es copia real, apunta al mismo array.

# Copia real
# a = np.array([1, 2, 3])
# b = a.copy()
# b[0] = 100

# print(a)  # no cambia
# print(b)
# 21. Casos prácticos para IA
# 21.1 Normalización simple

# Supongamos que tenés datos:

# x = np.array([10, 20, 30, 40, 50])
# Min-Max scaling
# x_norm = (x - np.min(x)) / (np.max(x) - np.min(x))
# print(x_norm)

# Salida:

# [0.   0.25 0.5  0.75 1.  ]
# 21.2 Estandarización
# x = np.array([10, 20, 30, 40, 50])

# x_std = (x - np.mean(x)) / np.std(x)
# print(x_std)

# Muy usada antes de entrenar modelos.

# 21.3 Dataset como matriz

# Cada fila es una muestra, cada columna una característica.

# X = np.array([
#     [170, 70],
#     [180, 80],
#     [160, 60]
# ])

# Podés separar columnas:

# altura = X[:, 0]
# peso = X[:, 1]
# 21.4 Predicción lineal simple

# Modelo:

# 𝑦
# =
# 𝑋
# 𝑤
# +
# 𝑏
# y=Xw+b

# En NumPy:

# X = np.array([
#     [1, 2],
#     [3, 4],
#     [5, 6]
# ])

# w = np.array([0.5, 1.0])
# b = 2

# y = X @ w + b
# print(y)

# Esto representa exactamente la base de muchos modelos.

# 21.5 ReLU y función sigmoide
# ReLU
# x = np.array([-2, -1, 0, 1, 2])
# relu = np.maximum(0, x)
# print(relu)
# Sigmoide
# x = np.array([-2, -1, 0, 1, 2])
# sigmoid = 1 / (1 + np.exp(-x))
# print(sigmoid)

# Estas funciones son fundamentales en redes neuronales.

# 21.6 Softmax
# x = np.array([2.0, 1.0, 0.1])

# exp_x = np.exp(x)
# softmax = exp_x / np.sum(exp_x)

# print(softmax)

# Versión más estable:

# x = np.array([2.0, 1.0, 0.1])

# exp_x = np.exp(x - np.max(x))
# softmax = exp_x / np.sum(exp_x)

# print(softmax)
# 22. Errores comunes
# 22.1 Confundir * con multiplicación matricial
# A * B

# hace multiplicación elemento a elemento.

# Para multiplicación matricial:

# A @ B

# o

# np.dot(A, B)
# 22.2 No revisar shape

# Muchos errores en IA vienen de dimensiones incompatibles.

# Siempre revisar:

# print(X.shape)
# print(w.shape)
# 22.3 Usar listas en vez de arrays

# Esto genera resultados inesperados.

# Mal:

# [1, 2, 3] * 2

# Bien:

# np.array([1, 2, 3]) * 2
# 22.4 Modificar una vista sin querer

# En slicing, a veces no se crea una copia independiente.

# Si querés independencia total:

# b = a[0:3].copy()
# 23. Buenas prácticas
# Importación estándar
# import numpy as np
# Revisar forma y tipo
# print(arr.shape)
# print(arr.dtype)
# Usar vectorización en vez de loops

# Menos eficiente:

# resultado = []
# for x in lista:
#     resultado.append(x * 2)

# Mejor:

# resultado = arr * 2
# Fijar semilla cuando haya aleatoriedad
# np.random.seed(42)
# Usar astype() cuando el tipo importe
# arr = arr.astype(np.float32)
# 24. Mini resumen mental para IA

# Pensalo así:

# vector → una fila o lista de números

# matriz → tabla de números

# array → estructura general de NumPy

# shape → tamaño/dimensiones

# axis → dirección en la que operás

# broadcasting → adaptación automática de dimensiones

# @ o dot → multiplicación matricial

# mean/std → normalización y estadística

# reshape → reorganizar datos

# masking → filtrar datos

# 25. Ejercicio completo integrador

# Este ejemplo junta varios conceptos.

# import numpy as np

# # 1. Dataset: filas = personas, columnas = [altura_cm, peso_kg]
# X = np.array([
#     [170, 70],
#     [180, 80],
#     [160, 60],
#     [175, 75]
# ], dtype=float)

# # 2. Ver forma
# print("Shape:", X.shape)

# # 3. Promedio por columna
# media = np.mean(X, axis=0)
# print("Media:", media)

# # 4. Desviación estándar por columna
# desvio = np.std(X, axis=0)
# print("Desvío:", desvio)

# # 5. Estandarización
# X_std = (X - media) / desvio
# print("Datos estandarizados:")
# print(X_std)

# # 6. Pesos y bias de un modelo lineal simple
# w = np.array([0.4, 0.6])
# b = 1.5

# # 7. Predicción
# y = X_std @ w + b
# print("Predicciones:")
# print(y)

# # 8. Aplicar ReLU
# y_relu = np.maximum(0, y)
# print("Predicciones con ReLU:")
# print(y_relu)

# Este ejemplo ya se parece bastante al flujo real de un modelo simple.

# 26. Ruta de aprendizaje recomendada

# Para dominar NumPy para IA, estudiá en este orden:

# Etapa 1

# creación de arrays

# shape, ndim, dtype

# indexación y slicing

# Etapa 2

# suma, resta, multiplicación, división

# funciones matemáticas

# estadísticas con axis

# Etapa 3

# broadcasting

# reshape

# transpuesta

# concatenación

# Etapa 4

# dot

# @

# np.linalg

# normalización y estandarización

# Etapa 5

# aplicar NumPy a mini modelos de IA

# regresión lineal

# funciones de activación

# softmax

# manejo de datasets

# 27. Qué deberías practicar sí o sí

# Te recomiendo practicar estos 10 puntos:

# crear arrays 1D, 2D y 3D

# usar shape, size, dtype

# seleccionar filas y columnas

# hacer operaciones entre arrays

# usar sum, mean, std con axis

# aplicar máscaras booleanas

# usar reshape y flatten

# entender broadcasting

# usar @ para multiplicación matricial

# implementar normalización, sigmoide y ReLU

# 28. Conclusión

# Si dominás NumPy, ya tenés una base muy fuerte para IA porque vas a entender:

# cómo se representan los datos

# cómo se transforman

# cómo se calculan predicciones

# cómo funcionan internamente muchas operaciones de machine learning

# NumPy es, en la práctica, la puerta de entrada a:

# Pandas

# Scikit-learn

# TensorFlow

# PyTorch

# visión por computadora

# deep learning

# Puedo seguir con la Parte 2: NumPy para IA con ejercicios resueltos y práctica real, así lo convertimos en una guía de estudio completa.