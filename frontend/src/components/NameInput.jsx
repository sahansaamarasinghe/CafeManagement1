const NameInput = ({ name, placeholder, onChange }) => {
  return (
    <>
      <input name={name} placeholder={placeholder} onChange={onChange} />
    </>
  );
};

export default NameInput;