import { createBrowserRouter } from "react-router-dom";
import AppLayout from "../layouts/AppLayout";
import Home from "../../pages/Home";
import NotFound from "../../pages/NotFound";
import Login from "../../pages/auth/Login";
import Register from "../../pages/auth/Register";
import ForgotPassword from "../../pages/auth/ForgotPassword";
import RequireAuth from "../../shared/components/auth/RequireAuth";
import Forbidden from "../../pages/Forbiddden";
import Checkout from "../../pages/Checkout";
// import ErrorTest from "../../shared/components/ErrorTest";
 export const router = createBrowserRouter([
  {
    path: "/",
    element: <AppLayout />,
    children: [
      {
        element: <RequireAuth roles={["Admin", "Customer"]} />,
  children: [{ index: true, element: <Home /> }, { path: "checkout", element: <Checkout /> }],
      },
      { path: "forbidden", element: <Forbidden /> },
      { path: "login", element: <Login /> },
      { path: "register", element: <Register /> },
      { path: "forgot-password", element: <ForgotPassword /> },
       //    { path: "error-test", element: <ErrorTest /> },

    ],
  },
  { path: "*", element: <NotFound /> },
]);
