import { Select, MenuItem, FormControl, FormHelperText } from '@mui/material';
import { styled } from '@mui/material/styles';

// Custom styled Select with white background, border, and black placeholder
const CustomSelect = styled(Select)(() => ({
  backgroundColor: '#fff',
  borderRadius: '8px',
  '& .MuiOutlinedInput-notchedOutline': {
    borderColor: '#e2e8f0',
    borderWidth: '2px',
    borderRadius: '8px',
  },
  '&:hover .MuiOutlinedInput-notchedOutline': {
    borderColor: 'rgba(0, 0, 0, 0.87)',
  },
  '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
    borderColor: '#667eea',
    borderWidth: '2px',
  },
  '& .MuiSelect-icon': {
    color: 'rgba(0, 0, 0, 0.54)',
  },
  '& .MuiSelect-select': {
    padding: '14px 20px',
  },
  '& .MuiSelect-select em': {
    color: '#718096',
    fontStyle: 'normal',
  },
}));

/**
 * Reusable styled Select component for consistent dropdown styling across the application
 * 
 * @param {Object} props - Component props
 * @param {string} props.label - Label text for the select
 * @param {string} props.id - ID for the select element
 * @param {string} props.name - Name attribute for the select
 * @param {*} props.value - Current value
 * @param {Function} props.onChange - Change handler
 * @param {boolean} props.required - Whether the field is required
 * @param {boolean} props.disabled - Whether the field is disabled
 * @param {boolean} props.error - Whether there's an error
 * @param {string} props.helperText - Helper/error text to display
 * @param {Array} props.options - Array of options {value, label}
 * @param {string} props.placeholder - Placeholder text for empty state
 */
export default function StyledSelect({
  label,
  id,
  name,
  value,
  onChange,
  required = false,
  disabled = false,
  error = false,
  helperText = '',
  options = [],
  placeholder = 'Select an option...',
  className = '',
}) {
  return (
    <FormControl fullWidth error={error} disabled={disabled} className={className}>
      {label && (
        <label htmlFor={id} className="whistler-text" style={{ marginBottom: '8px', display: 'block' }}>
          {label}
        </label>
      )}
      <CustomSelect
        id={id}
        name={name}
        value={value}
        onChange={onChange}
        required={required}
        displayEmpty
      >
        <MenuItem value="">
          <em>{placeholder}</em>
        </MenuItem>
        {options.map((option) => (
          <MenuItem key={option.value} value={option.value}>
            {option.label}
          </MenuItem>
        ))}
      </CustomSelect>
      {helperText && (
        <FormHelperText className="whistler-text">{helperText}</FormHelperText>
      )}
    </FormControl>
  );
}
