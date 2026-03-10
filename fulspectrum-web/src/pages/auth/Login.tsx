import { type FormEvent, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useLoginMutation } from "../../state/api";
import { useAppDispatch } from "../../state/hooks";
import { setCredentials } from "../../state/authSlice";

export default function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [login, { isLoading }] = useLoginMutation();
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const result = await login({ email, password });
    if ("data" in result && result.data) {
      dispatch(setCredentials({ accessToken: result.data.accessToken, profile: result.data.profile }));
      navigate("/");
    }
  };

  return (
    <form onSubmit={onSubmit} className="max-w-md mx-auto mt-8 space-y-3 border rounded p-4">
      <h1 className="text-xl font-semibold">Iniciar sesión</h1>
      <input className="w-full border rounded px-3 py-2" placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
      <input className="w-full border rounded px-3 py-2" type="password" placeholder="Contraseña" value={password} onChange={(e) => setPassword(e.target.value)} />
      <button className="border rounded px-3 py-2" disabled={isLoading}>{isLoading ? "Entrando..." : "Entrar"}</button>
      <p className="text-sm">¿No tienes cuenta? <Link className="underline" to="/register">Regístrate</Link></p>
      <p className="text-sm"><Link className="underline" to="/forgot-password">Olvidé mi contraseña</Link></p>
    </form>
  );
}
