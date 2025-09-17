/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{js,jsx,ts,tsx}",   // tất cả file trong thư mục src
  ],
  theme: {
    extend: {},
  },
  plugins: [
    require("@tailwindcss/forms"),  // plugin hỗ trợ form đẹp hơn (nếu bạn muốn)
  ],
}
