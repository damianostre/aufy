import {ResetPasswordForm} from "../components/ResetPasswordForm.tsx";

export const ResetPassword = () => {
    return (
        <>
            <div className="flex min-h-full flex-1 flex-col justify-center py-6 sm:px-6 lg:px-8">
                <div className="sm:mx-auto sm:w-full sm:max-w-md">
                    <h2 className="mt-6 text-center text-2xl font-bold leading-9 tracking-tight text-gray-900">
                        Reset your password
                    </h2>
                </div>

                <div className="mt-10 sm:mx-auto sm:w-full sm:max-w-[480px]">
                    <div className="bg-white px-6 py-8 shadow sm:rounded-lg sm:px-12">
                        <ResetPasswordForm />
                    </div>
                </div>
            </div>
        </>
    );
};