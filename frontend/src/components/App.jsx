import React from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";


import LoginPage from "../pages/LoginPage";
import SignUpPage from "../pages/SignUpPage";
import HomePage from "../pages/HomePage";

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<SignUpPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/home" element={<HomePage />} />
      </Routes>
    </Router>
  );
}

export default App;
