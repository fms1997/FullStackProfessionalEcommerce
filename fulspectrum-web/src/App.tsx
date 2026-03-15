import { Navigate, Route, Routes } from "react-router-dom";
import { PaymentSuccessPage } from "./pages/PaymentSuccessPage";
import { PaymentFailPage } from "./pages/PaymentFailPage";
import { PaymentPendingPage } from "./pages/PaymentPendingPage";
 
type AppProps = {
  token: string;
};

export function App({ token }: AppProps) {
  return (
    <Routes>
      <Route path="/payment/success" element={<PaymentSuccessPage />} />
      <Route path="/payment/fail" element={<PaymentFailPage token={token} />} />
      <Route path="/payment/pending" element={<PaymentPendingPage token={token} />} />
      <Route path="*" element={<Navigate to="/payment/pending" replace />} />
    </Routes>
  );
}
