import { type FormEvent, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useRegisterMutation } from "../../state/api";
import { useAppDispatch } from "../../state/hooks";
import { setCredentials } from "../../state/authSlice";

export default function Register() {
  const [form, setForm] = useState({ firstName: "", lastName: "", email: "", password: "" });
  const [register, { isLoading }] = useRegisterMutation();
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const result = await register(form);
    if ("data" in result && result.data) {
      dispatch(setCredentials({ accessToken: result.data.accessToken, profile: result.data.profile }));
      navigate("/");
    }
  };

  return (
    <form onSubmit={onSubmit} className="max-w-md mx-auto mt-8 space-y-3 border rounded p-4">
      <h1 className="text-xl font-semibold">Crear cuenta</h1>
      <input className="w-full border rounded px-3 py-2" placeholder="Nombre" value={form.firstName} onChange={(e) => setForm((x) => ({ ...x, firstName: e.target.value }))} />
      <input className="w-full border rounded px-3 py-2" placeholder="Apellido" value={form.lastName} onChange={(e) => setForm((x) => ({ ...x, lastName: e.target.value }))} />
      <input className="w-full border rounded px-3 py-2" placeholder="Email" value={form.email} onChange={(e) => setForm((x) => ({ ...x, email: e.target.value }))} />
      <input className="w-full border rounded px-3 py-2" type="password" placeholder="Contraseña" value={form.password} onChange={(e) => setForm((x) => ({ ...x, password: e.target.value }))} />
      <button className="border rounded px-3 py-2" disabled={isLoading}>{isLoading ? "Creando..." : "Registrarme"}</button>
      <p className="text-sm">¿Ya tienes cuenta? <Link className="underline" to="/login">Inicia sesión</Link></p>
    </form>
  );
}
