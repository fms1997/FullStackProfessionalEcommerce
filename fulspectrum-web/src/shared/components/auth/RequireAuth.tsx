import { Navigate, Outlet } from "react-router-dom";
import { useAppSelector } from "../../../state/hooks";

type Props = {
  roles?: Array<"Admin" | "Customer">;
};

export default function RequireAuth({ roles }: Props) {
  const { accessToken, profile } = useAppSelector((s) => s.auth);

  if (!accessToken || !profile) {
    return <Navigate to="/login" replace />;
  }

  if (roles && !roles.includes(profile.role)) {
    return <Navigate to="/forbidden" replace />;
  }

  return <Outlet />;
}
