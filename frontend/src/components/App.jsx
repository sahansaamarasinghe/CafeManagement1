import React from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";


import Login from "./Login";
import SignUpPage from "./SignUpPage";

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<SignUpPage />} />
        <Route path="/login" element={<Login />} />
      </Routes>
    </Router>
  );
}

export default App;
