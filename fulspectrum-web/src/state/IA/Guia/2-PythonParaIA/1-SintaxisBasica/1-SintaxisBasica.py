# 1- Variables:

nombre = "Franco"
edad = 30
altura = 1.75
es_programador = True

print(type(nombre))
print(type(edad))


# 2-Listas:
numeros = [1,2,3,4,5]
nombres = ["Ana","Juan","Pedro"]

print(numeros[0])  # 1
print(nombres[1])  # Juan

numeros[0] = 10
numeros.append(6)

len(numeros)

for n in numeros:
    print(n)



# 3. Bucles (Loops)
for i in range(5):
    print(i)

    frutas = ["manzana","banana","pera"]

for fruta in frutas:
    print(fruta)


    contador = 0

while contador < 5:
    print(contador)
    contador += 1


    # 4. Condicionales
    edad = 18

if edad >= 18:
    print("Mayor de edad")
else:
    print("Menor de edad")


    # 5. Funciones

def saludar():
    print("Hola")
saludar()

def saludar(nombre):
    print("Hola", nombre)

saludar("Franco")


def sumar(a, b):
    return a + b

resultado = sumar(5,3)
print(resultado)



# 6. Diccionarios (muy usados en IA)

persona = {
    "nombre": "Franco",
    "edad": 30,
    "pais": "Argentina"
}

print(persona["nombre"])

persona["profesion"] = "Programador"


# 7. List Comprehension (MUY usado en IA)
numeros = [1,2,3,4,5]

cuadrados = [n**2 for n in numeros]

print(cuadrados)

# 8. Librerías clave para IA
# Librería	Uso
# numpy	cálculos matemáticos
# pandas	análisis de datos
# matplotlib	gráficos
# scikit-learn	machine learning
# tensorflow	deep learning
# pytorch	deep learning


# 9. Ejemplo simple estilo IA
datos = [10,20,30,40,50]

def promedio(lista):
    return sum(lista) / len(lista)

print(promedio(datos))

# 10. Estructura típica de proyecto de IA
# proyecto_ia/
# │
# ├── data/
# │
# ├── notebooks/
# │
# ├── models/
# │
# ├── train.py
# │
# └── predict.py

# Si querés aprender IA rápido

# Este sería el orden ideal:

# 1️⃣ Python básico
# 2️⃣ Numpy
# 3️⃣ Pandas
# 4️⃣ Visualización
# 5️⃣ Machine Learning
# 6️⃣ Deep Learning