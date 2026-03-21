import { Suspense, lazy, type ReactNode } from "react";
import { createBrowserRouter } from "react-router-dom";

import AppLayout from "../layouts/AppLayout";
import RequireAuth from "../../shared/components/auth/RequireAuth";

const Home = lazy(() => import("../../pages/Home"));
const NotFound = lazy(() => import("../../pages/NotFound"));
const Login = lazy(() => import("../../pages/auth/Login"));
const Register = lazy(() => import("../../pages/auth/Register"));
const ForgotPassword = lazy(() => import("../../pages/auth/ForgotPassword"));
const Forbidden = lazy(() => import("../../pages/Forbiddden"));
const Checkout = lazy(() => import("../../pages/Checkout"));
const MyOrders = lazy(() => import("../../pages/MyOrders"));
const OrderDetail = lazy(() => import("../../pages/OrderDetail"));
const AdminPanel = lazy(() => import("../../pages/AdminPanel"));

const PaymentPendingPage = lazy(() =>
  import("../../pages/PaymentPendingPage").then((m) => ({
    default: m.PaymentPendingPage,
  })),
);

const PaymentSuccessPage = lazy(() =>
  import("../../pages/PaymentSuccessPage").then((m) => ({
    default: m.PaymentSuccessPage,
  })),
);

const PaymentFailPage = lazy(() =>
  import("../../pages/PaymentFailPage").then((m) => ({
    default: m.PaymentFailPage,
  })),
);

const withSuspense = (node: ReactNode) => (
  <Suspense fallback={<div className="p-4 text-sm">Cargando módulo...</div>}>
    {node}
  </Suspense>
);

export const router = createBrowserRouter([
  {
    path: "/",
    element: <AppLayout />,
    children: [
      {
        element: <RequireAuth roles={["Admin", "Customer"]} />,
        children: [
          { index: true, element: withSuspense(<Home />) },
          { path: "checkout", element: withSuspense(<Checkout />) },
          { path: "orders", element: withSuspense(<MyOrders />) },
          { path: "orders/:orderId", element: withSuspense(<OrderDetail />) },
          {
            path: "payment/pending",
            element: withSuspense(<PaymentPendingPage />),
          },
          {
            path: "payment/success",
            element: withSuspense(<PaymentSuccessPage />),
          },
          {
            path: "payment/fail",
            element: withSuspense(<PaymentFailPage />),
          },
          {
            element: <RequireAuth roles={["Admin"]} />,
            children: [
              {
                path: "admin",
                element: withSuspense(<AdminPanel />),
              },
            ],
          },
        ],
      },
      { path: "forbidden", element: withSuspense(<Forbidden />) },
      { path: "login", element: withSuspense(<Login />) },
      { path: "register", element: withSuspense(<Register />) },
      {
        path: "forgot-password",
        element: withSuspense(<ForgotPassword />),
      },
    ],
  },
  { path: "*", element: withSuspense(<NotFound />) },
]);