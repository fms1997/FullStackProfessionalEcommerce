import { createBrowserRouter } from "react-router-dom";
import AppLayout from "../layouts/AppLayout";
import Home from "../../pages/Home";
import NotFound from "../../pages/NotFound";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <AppLayout />,
    children: [{ index: true, element: <Home /> }],
  },
  { path: "*", element: <NotFound /> },
]);