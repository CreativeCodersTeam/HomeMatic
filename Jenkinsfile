@Library('JenkinsPipelineLibrary@develop') _

pipeline {
    agent any

    triggers { 
        githubPush()
    }

    environment {
        NUGET_DEV_API_KEY = credentials('nuget.dev.apikey')
        NUGET_ORG_API_KEY = credentials('nuget.org.apikey')
    }

    stages {
        stage("DotNet") {
            stages {
                stage("Build") {
                    steps {                        
                        echo 'Build Core Solution'
                        
                        nuke(target: 'runbuild')
                    }
                }
                stage("Test") {
                    steps {
                        echo 'Run tests'

                        sh 'nuke test'
                    }
                    post {
                        always {
                            publishXUnitResults()
                        }
                    }
                }
                stage("Pack") {
                    steps {
                        echo 'Run tests'

                        sh 'nuke pack'
                    }
                }
            }
            post {
                failure {
                    echo "========A ${STAGE_NAME} execution failed========"
                }
            }
        }
        stage("DeployFeatureDevNuGet") {
            when {
                branch 'feature/*'
            }            
            steps {
                echo "Deploy feature NuGet packages to dev NuGet repository"

                nuke(target: 'pushtodevnuget', devnugetsource: env.DEV_NUGET_URL, devnugetapikey: env.NUGET_DEV_API_KEY)
            }            
        }
        stage("DeployDevNuGet") {
            when{
                branch 'develop'
            }
            environment {
                NUGET_DEV_API_KEY = credentials('nuget.dev.apikey')
            }
            steps {
                echo "Deploy NuGet packages to dev NuGet repository"

                nuke(target: 'deploytodevnuget', devnugetsource: env.DEV_NUGET_URL, devnugetapikey: env.NUGET_DEV_API_KEY)
            }            
        }
        stage("DeployNuGet") {
            when {
                branch 'master'
            }
            steps {
                echo "Deploy NuGet packages to NuGet.org repository"

                nuke(target: 'deploytonuget', nugetsource: 'https://api.nuget.org/v3/index.json', nugetapikey: env.NUGET_ORG_API_KEY)
            }            
        }
    }
    post {
        success {
            echo "========pipeline executed successfully ========"
        }
        failure {
            echo "========pipeline execution failed========"
        }
    }
}