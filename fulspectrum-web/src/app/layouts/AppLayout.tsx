import { Link, Outlet, useNavigate } from "react-router-dom";
import { useLogoutMutation, useMergeCartMutation } from "../../state/api";
import { clearAuth } from "../../state/authSlice";
import { useAppDispatch, useAppSelector } from "../../state/hooks";
import { clearLocalCart } from "../../state/cartSlice";
import { useEffect } from "react";
export default function AppLayout() {
  const { profile } = useAppSelector((s) => s.auth);
  const { localItems, serverCart } = useAppSelector((s) => s.cart);
  const [logout] = useLogoutMutation();
  const [mergeCart] = useMergeCartMutation();
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  useEffect(() => {
    const merge = async () => {
      if (!profile || localItems.length === 0) return;
      const result = await mergeCart({
        items: localItems.map((item) => ({
          productId: item.productId,
          quantity: item.quantity,
        })),
        rowVersion: serverCart?.rowVersion,
      });

      if ("data" in result && result.data) {
        dispatch(clearLocalCart());
      }
    };

    merge();
  }, [profile, localItems, mergeCart, dispatch, serverCart?.rowVersion]);

  const onLogout = async () => {
    await logout();
    dispatch(clearAuth());
    navigate("/login");
  };
  return (
    <div className="min-h-screen">
      <header className="border-b p-4 flex items-center justify-between">
         <div className="font-semibold">FulSpectrum</div>
        <nav className="flex items-center gap-3 text-sm">
          <Link to="/">Inicio</Link>
                    {profile && <Link to="/checkout">Checkout</Link>}
                                        {profile && <Link to="/orders">Mis pedidos</Link>}
          {!profile ? (
            <>
              <Link to="/login">Login</Link>
              <Link to="/register">Registro</Link>
            </>
          ) : (
            <>
              <span>
                {profile.email} ({profile.role})
              </span>
              <button onClick={onLogout} className="underline">
                Salir
              </button>{" "}
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
