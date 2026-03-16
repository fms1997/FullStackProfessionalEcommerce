import { Navigate, Route, Routes } from "react-router-dom";
import { PaymentSuccessPage } from "./pages/PaymentSuccessPage";
import { PaymentFailPage } from "./pages/PaymentFailPage";
import { PaymentPendingPage } from "./pages/PaymentPendingPage";
 

export function App() {
  return (
    <Routes>
      <Route path="/payment/success" element={<PaymentSuccessPage />} />
         <Route path="/payment/fail" element={<PaymentFailPage />} />
      <Route path="/payment/pending" element={<PaymentPendingPage />} />
           <Route path="*" element={<Navigate to="/payment/pending" replace />} />
    </Routes>
  );
}
