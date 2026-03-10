import { createBrowserRouter } from "react-router-dom";
import AppLayout from "../layouts/AppLayout";
import Home from "../../pages/Home";
import NotFound from "../../pages/NotFound";
import Login from "../../pages/auth/Login";
import Register from "../../pages/auth/Register";
import ForgotPassword from "../../pages/auth/ForgotPassword";
import Forbidden from "../../pages/Forbidden";
import RequireAuth from "../../shared/components/auth/RequireAuth";
export const router = createBrowserRouter([
  {
    path: "/",
    element: <AppLayout />,
 children: [
      {
        element: <RequireAuth roles={["Admin", "Customer"]} />,
        children: [{ index: true, element: <Home /> }],
      },
      { path: "forbidden", element: <Forbidden /> },
      { path: "login", element: <Login /> },
      { path: "register", element: <Register /> },
      { path: "forgot-password", element: <ForgotPassword /> },
    ],  },
  { path: "*", element: <NotFound /> },
]);