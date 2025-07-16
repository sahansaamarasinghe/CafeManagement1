import { useState } from "react";
import axios from "axios";
import NameInput from "./NameInput";
const api = import.meta.env.VITE_API_URL;

function SignUpPage() {
  const [form, setForm] = useState({
    fullName: "",
    userName: "",
    email: "",
    password: "",
    confirmPassword: "",
  });

  const handleChange = (e) => {
    setForm({ ...form, [e?.target?.name]: e?.target?.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    // try {
    //   console.log("Submitting form data:", form);

    //   const res = await axios.post("https://localhost:7178/api/auth/register", form);5536

    //   alert("Registration successful");
    // } catch (err) {
    //   console.error(err);
    //   alert("Registration failed");/
    // }
    //axios.post(`${api}/Auth/register`, form);
    axios
      .post(`${api}/Auth/register`, form)
      .then((res) => {
        console.log("Success", res.data);
      })
      .catch((err) => {
        console.log("Error:", err);
      });
  };

  return (
    <div>
      <h2>Register</h2>
      <form onSubmit={handleSubmit}>
        <div className="input-group">
          <NameInput
            name="fullName"
            placeholder="Full Name"
            onChange={handleChange}
          />
          <NameInput
            name="userName"
            placeholder="User Name"
            onChange={handleChange}
          />
          <NameInput name="email" placeholder="Email" onChange={handleChange} />
          <NameInput
            name="password"
            placeholder="Password"
            onChange={handleChange}
          />
          <NameInput
            name="confirmPassword"
            placeholder="Confirm Password"
            onChange={handleChange}
          />
        </div>
        <button type="submit">Register</button>
      </form>
    </div>
  );
}

export default SignUpPage;
