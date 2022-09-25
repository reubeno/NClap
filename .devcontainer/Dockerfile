FROM mcr.microsoft.com/vscode/devcontainers/dotnet:6.0-focal

# Install native components
RUN apt-get update && \
    apt-get install -y \
        curl \
        dos2unix \
        git \
        gnupg2 \
        sudo \
        zsh \
        zsh-syntax-highlighting \
        zsh-autosuggestions

# Switch to non-root user
USER vscode

# Select oh my zsh plugins
# Install 'bat' from GitHub release drop
# Install 'exa' from GitHub release drop
RUN sed -i "s/^plugins=.*/plugins=(git zsh-autosuggestions zsh-syntax-highlighting)/" ~/.zshrc && \
    \
    mkdir -p /tmp/work && \
    cd /tmp/work && \
    \
    wget https://github.com/sharkdp/bat/releases/download/v0.22.1/bat-musl_0.22.1_amd64.deb -O ./bat.deb && \
    sudo dpkg -i ./bat.deb && \
    sudo apt-get install -f && \
    \
    wget https://github.com/ogham/exa/releases/download/v0.10.1/exa-linux-x86_64-v0.10.1.zip -O ./exa.zip && \
    unzip ./exa.zip && \
    sudo mv ./bin/* /usr/bin && \
    sudo mv ./completions/exa.zsh /usr/share/zsh/vendor-completions/_exa && \
    \
    cd && \
    rm -rf /tmp/work
