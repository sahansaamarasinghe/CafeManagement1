import { useState } from "react";
import axios from "axios";
const api = import.meta.env.VITE_API_URL;
import FormInput from "../components/FormInput";
import "../styles/index.css";
import { useNavigate } from "react-router-dom";
import "../styles/AuthForm.css";

function LoginPage() {
  const [form, setForm] = useState({
    email: "",
    password: "",
  });

  const navigate = useNavigate();
  const [error, setError] = useState("");
  const [loggedInUser, setLoggedInUser] = useState(null);

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    try {
      const res = await axios.post(`${api}/Auth/login`, form);
      const { Username, Token, Roles } = res.data;

      localStorage.setItem("token", Token);
      localStorage.setItem("username", Username);
      localStorage.setItem("roles", JSON.stringify(Roles));

      setLoggedInUser(Username);

      navigate("/Home");

      alert("Login successful");
      console.log("Token:", Token);
      console.log("Roles:", Roles);
    } catch (err) {
      console.error("Login error:", err);
      const message =
        err.response?.data || "Login failed. Please check your credentials.";
      setError(message);
    }
  };

const styles = {}

  return (
    <div style={{ maxWidth: "800px", margin: "auto" }}>
      <div className="auth-page" style={styles}>
        <button
          type="button"
          className="link-button"
          onClick={() => navigate("/")}
        >
          Go To Sign Up
        </button>
        <div className="auth-form-container">
          <h2>Login</h2>
          <form onSubmit={handleSubmit}>
            <div className="input-group">
              <FormInput
                name="email"
                type="email"
                placeholder="Email"
                value={form.email}
                onChange={handleChange}
                required
              />
              <FormInput
                name="password"
                type="password"
                placeholder="Password"
                value={form.password}
                onChange={handleChange}
                required
              />
            </div>
            <button type="submit">Login</button>
          </form>

          {error && <p style={{ color: "red" }}>{error}</p>}
          {loggedInUser && (
            <p style={{ color: "green" }}>Welcome, {loggedInUser}!</p>
          )}
        </div>
      </div>
    </div>
  );
}

export default LoginPage;
