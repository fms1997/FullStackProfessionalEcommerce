import { Link, Outlet, useNavigate } from "react-router-dom";
import { useLogoutMutation } from "../../state/api";
import { clearAuth } from "../../state/authSlice";
import { useAppDispatch, useAppSelector } from "../../state/hooks";
export default function AppLayout() {
    const { profile } = useAppSelector((s) => s.auth);
  const [logout] = useLogoutMutation();
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const onLogout = async () => {
    await logout();
    dispatch(clearAuth());
    navigate("/login");
  };
  return (
    <div className="min-h-screen">
      <header className="border-b p-4 flex items-center justify-between">        <div className="font-semibold">FulSpectrum</div>
     <div className="font-semibold">FulSpectrum</div>
        <nav className="flex items-center gap-3 text-sm">
          <Link to="/">Inicio</Link>
          {!profile ? (
            <>
              <Link to="/login">Login</Link>
              <Link to="/register">Registro</Link>
            </>
          ) : (
            <>
              <span>{profile.email} ({profile.role})</span>
              <button onClick={onLogout} className="underline">Salir</button>
            </>
          )}
        </nav>
      </header>
      <main>
        <Outlet />
      </main>
    </div>
  );
}