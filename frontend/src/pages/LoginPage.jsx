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
  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const res = await axios.post(`${api}/Auth/login`, form);
      const { Username, Token, Roles } = res.data;

      if (Token) {
        localStorage.setItem("token", Token);
        localStorage.setItem("username", Username);
        localStorage.setItem("roles", JSON.stringify(Roles));

        setLoggedInUser(Username);
        navigate("/Home");
        console.log("Token:", Token);
        console.log("Roles:", Roles);
      } else {
        setError("Unexpected response from server.");
      }
    } catch (err) {
      console.error("Login error:", err);
      const message =
        err.response?.data?.message ||
        err.response?.data ||
        "Login failed. Please check your credentials or try again later.";
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ maxWidth: "800px", margin: "auto" }}>
      <div className="auth-page">
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
            <button type="submit" disabled={loading}>
              {loading ? "Logging in..." : "Login"}
            </button>
          </form>

          {error && <p style={{ color: "red", marginTop: "1rem" }}>{error}</p>}
          {loggedInUser && (
            <p style={{ color: "green", marginTop: "1rem" }}>
              Welcome, {loggedInUser}!
            </p>
          )}

          <p style={{ textAlign: "center", marginTop: "1rem" }}>
            Don't have an account?{" "}
            <span
              onClick={() => navigate("/")}
              style={{
                color: "#6f4e37",
                cursor: "pointer",
                textDecoration: "underline",
              }}
            >
              Register here
            </span>
          </p>
        </div>
      </div>
    </div>
  );
}

export default LoginPage;
