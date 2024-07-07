import {AppProvider} from "./providers/app.tsx";
import {AppRoutes} from "./routes";

function App() {
    return (
        <AppProvider>
            <AppRoutes />
        </AppProvider>
    );
}

export default App;
