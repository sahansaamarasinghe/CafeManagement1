function FormInput({ name, type = "text", placeholder, value, onChange, required }) {
  return (
    <input
      name={name}
      type={type}
      placeholder={placeholder}
      value={value}
      onChange={onChange}
      required={required}
    />
  );
}

export default FormInput;