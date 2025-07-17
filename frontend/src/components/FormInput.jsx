function FormInput({ name, type = "text", placeholder, value, onChange, required }) {
  return (
    <input
      name={name}
      type={type}
      placeholder={placeholder}
      value={value}
      onChange={onChange}
      required={required}
      style={{justifyContent:"center"}}
    />
  );
}

export default FormInput;